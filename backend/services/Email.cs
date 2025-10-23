using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Models;
using Microsoft.EntityFrameworkCore;
namespace backend.services
{
  // Minimal templating: replace {{Key}} with model["Key"].ToString()
  public interface IEmailService
  {
    Task SendTemplatedEmailAsync(string to, string subjectTemplate, string htmlTemplate, IDictionary<string, object?> model);
    Task NotifyBudgetsForTransactionAsync(Transaction tx, FinancetrackerContext db, Microsoft.Extensions.Logging.ILogger? logger);
  }

  public class MailjetEmailService : IEmailService
  {
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly Microsoft.Extensions.Logging.ILogger<MailjetEmailService> _logger;

    private static readonly DateTime SqlServerMin = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
    private static readonly DateTime SqlServerMax = System.Data.SqlTypes.SqlDateTime.MaxValue.Value;

    public MailjetEmailService(IHttpClientFactory httpFactory, IConfiguration config, Microsoft.Extensions.Logging.ILogger<MailjetEmailService> logger)
    {
      _httpFactory = httpFactory;
      _config = config;
      _logger = logger;
    }

    private static string RenderTemplate(string template, IDictionary<string, object?> model)
    {
      if (string.IsNullOrEmpty(template) || model == null || model.Count == 0)
        return template ?? string.Empty;

      var sb = new StringBuilder(template);
      foreach (var kv in model)
      {
        var placeholder = "{{" + kv.Key + "}}";
        var val = kv.Value?.ToString() ?? string.Empty;
        sb.Replace(placeholder, val);
      }
      return sb.ToString();
    }

    public async Task SendTemplatedEmailAsync(string to, string subjectTemplate, string htmlTemplate, IDictionary<string, object?> model)
    {
      var apiKey = _config["Email:Mailjet:ApiKey"];
      var apiSecret = _config["Email:Mailjet:ApiSecret"];
      var fromEmail = _config["Email:From"];
      var fromName = _config["Email:FromName"] ?? "FinanceTracker";

      if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(fromEmail))
      {
        // not configured, no-op
        return;
      }

      var subject = RenderTemplate(subjectTemplate ?? string.Empty, model);
      var html = RenderTemplate(htmlTemplate ?? string.Empty, model);
      var plain = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

      var client = _httpFactory.CreateClient("mailjet");

      var mailjetBase = _config["Email:Mailjet:BaseUrl"] ?? "https://api.mailjet.com/v3.1/send";

      var payload = new
      {
        Messages = new[]
        {
          new
          {
            From = new { Email = fromEmail, Name = fromName },
            To = new[] { new { Email = to } },
            Subject = subject,
            TextPart = plain,
            HTMLPart = html
          }
        }
      };

      var json = JsonSerializer.Serialize(payload);
      var req = new HttpRequestMessage(HttpMethod.Post, mailjetBase)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };

      var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
      req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

      // log payload at debug level (may contain PII, so only debug)
      _logger.LogDebug("Mailjet request payload: {Payload}", json);

      var res = await client.SendAsync(req);
      var respBody = string.Empty;
      try { respBody = await res.Content.ReadAsStringAsync(); } catch (Exception ex) { _logger.LogDebug(ex, "Failed to read Mailjet response body."); }

      if (res.IsSuccessStatusCode)
      {
        // Mailjet returns message info in the body; log it for debugging
        _logger.LogInformation("Mailjet API returned success for {To}: {Status} {Body}", to, res.StatusCode, respBody);
      }
      else
      {
        _logger.LogWarning("Mailjet send failed for {To}: {Status} {Body}", to, res.StatusCode, respBody);
      }
    }
    public async Task NotifyBudgetsForTransactionAsync(Transaction tx, FinancetrackerContext db, Microsoft.Extensions.Logging.ILogger? logger)
    {
      var log = logger ?? _logger;
      try
      {
        if (tx == null || db == null)
          return;

        var userId = tx.UserId;
        if (userId == Guid.Empty)
          return;

        // load budgets for user that either are global (CategoryId == null) or match the tx.CategoryId
        var budgets = await db.Budgets.Include(b => b.Category)
          .Where(b => b.UserId == userId && (b.CategoryId == null || b.CategoryId == tx.CategoryId))
          .ToListAsync();
        log?.LogDebug("Evaluating {Count} budgets for category {CategoryId} (including global) and user {UserId}", budgets.Count, tx.CategoryId, userId);

        const string UnnamedLabel = "unnamed";
        foreach (var b in budgets)
        {
          var start = b.StartDate ?? SqlServerMin;
          var end = b.EndDate ?? SqlServerMax;

          // gather relevant transactions
          var relevant = await db.Transactions.Include(t => t.Category)
            .Where(t => t.UserId == userId
              && (b.CategoryId == null || t.CategoryId == b.CategoryId)
              && ((t.Date ?? t.CreatedAt ?? SqlServerMin) >= start)
              && ((t.Date ?? t.CreatedAt ?? SqlServerMin) <= end))
            .ToListAsync();

          var spent = relevant
            .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
            .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

          // compute spent before this transaction (exclude this tx) so we only notify when crossing thresholds
          var spentBefore = relevant
            .Where(t => t.TransactionId != tx.TransactionId)
            .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
            .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

          log?.LogDebug("Budget {BudgetId} evaluation: spent={Spent} amount={Amount}", b.BudgetId, spent, b.Amount);

          if (b.Amount <= 0)
            continue;

          var percent = (int)Math.Floor((spent / b.Amount) * 100);
          var percentBefore = (int)Math.Floor((spentBefore / b.Amount) * 100);

          // Remove stale notifications for this budget if budget period changed
          var stale = await db.Notifications.Where(n => n.BudgetId == b.BudgetId && (n.PeriodStart != b.StartDate || n.PeriodEnd != b.EndDate)).ToListAsync();
          if (stale.Count > 0)
          {
            db.Notifications.RemoveRange(stale);
            await db.SaveChangesAsync();
            log?.LogDebug("Removed {Count} stale notification(s) for budget {BudgetId}", stale.Count, b.BudgetId);
          }

          // check thresholds 90 and 100 (persistent check prevents duplicate across restarts)
          if (percent >= 90 && percent < 100 && percentBefore < 90)
          {
            var existing = await db.Notifications.AnyAsync(n => n.BudgetId == b.BudgetId && n.Level == 90 && n.PeriodStart == b.StartDate && n.PeriodEnd == b.EndDate);
            if (!existing)
            {
              var user = await db.Users.Where(u => u.UserId == b.UserId).FirstOrDefaultAsync();
              if (user != null && !string.IsNullOrWhiteSpace(user.Email))
              {
                var subjectTemplate = "Budget '{{BudgetName}}' is {{Percent}}% used";
                var htmlTemplate = "<p>Hi,</p><p>Your budget '<strong>{{BudgetName}}</strong>' has used <strong>{{Percent}}%</strong> of its allocated amount ({{Spent}} of {{Amount}}).</p><p>Regards,<br/>Trackaroo team</p>";
                var model = new Dictionary<string, object?>
                {
                  ["BudgetName"] = b.Name ?? UnnamedLabel,
                  ["Percent"] = percent,
                  ["Spent"] = (long)Math.Round(spent),
                  ["Amount"] = (long)Math.Round(b.Amount)
                };
                try
                {
                  await SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
                  db.Notifications.Add(new Notification { BudgetId = b.BudgetId, Level = 90, SentAt = DateTime.UtcNow, TransactionId = tx.TransactionId, PeriodStart = b.StartDate, PeriodEnd = b.EndDate });
                  await db.SaveChangesAsync();
                  log?.LogInformation("Sent 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
                }
                catch (Exception ex)
                {
                  log?.LogWarning(ex, "Failed to send 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
                }
              }
            }
            else
            {
              log?.LogDebug("Skipping 90% email for budget {BudgetId} as persistent record exists", b.BudgetId);
            }
          }

          if (percent >= 100 && percentBefore < 100)
          {
            var existing = await db.Notifications.AnyAsync(n => n.BudgetId == b.BudgetId && n.Level == 100 && n.PeriodStart == b.StartDate && n.PeriodEnd == b.EndDate);
            if (!existing)
            {
              var user = await db.Users.Where(u => u.UserId == b.UserId).FirstOrDefaultAsync();
              if (user != null && !string.IsNullOrWhiteSpace(user.Email))
              {
                var subjectTemplate = "Budget '{{BudgetName}}' reached 100%";
                var htmlTemplate = "<p>Hi,</p><p>Your budget '<strong>{{BudgetName}}</strong>' has reached or exceeded its allocated amount. Spent: {{Spent}} of {{Amount}}.</p><p>Regards,<br/>Trackaroo team</p>";
                var model = new Dictionary<string, object?>
                {
                  ["BudgetName"] = b.Name ?? UnnamedLabel,
                  ["Percent"] = percent,
                  ["Spent"] = (long)Math.Round(spent),
                  ["Amount"] = (long)Math.Round(b.Amount)
                };
                try
                {
                  await SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
                  db.Notifications.Add(new Notification { BudgetId = b.BudgetId, Level = 100, SentAt = DateTime.UtcNow, TransactionId = tx.TransactionId, PeriodStart = b.StartDate, PeriodEnd = b.EndDate });
                  await db.SaveChangesAsync();
                  log?.LogInformation("Sent 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
                }
                catch (Exception ex)
                {
                  log?.LogWarning(ex, "Failed to send 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
                }
              }
            }
            else
            {
              log?.LogDebug("Skipping 100% email for budget {BudgetId} as persistent record exists", b.BudgetId);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while evaluating budgets for notifications.");
      }
    }
  }
}