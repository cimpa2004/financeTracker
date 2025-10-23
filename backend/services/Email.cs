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
    // Simple in-memory dedup to avoid duplicate notifications for the same budget level during app lifetime.
    // Key = budgetId, value = highest level notified (0 = none, 90, 100)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, int> _sentNotifications = new();

    public static async Task TryNotifyBudgetsForTransactionAsync(Transaction tx, FinancetrackerContext db, backend.services.IEmailService? emailService, Microsoft.Extensions.Logging.ILogger? logger)
    {
      try
      {
        if (emailService == null)
          return; // no email configured or not injected

        var userId = tx.UserId;
        if (userId == Guid.Empty)
          return;

        // load budgets for user that either are global (CategoryId == null) or match the tx.CategoryId
        var budgets = await db.Budgets.Include(b => b.Category)
          .Where(b => b.UserId == userId && (b.CategoryId == null || b.CategoryId == tx.CategoryId))
          .ToListAsync();
        logger?.LogDebug("Evaluating {Count} budgets for category {CategoryId} (including global) and user {UserId}", budgets.Count, tx.CategoryId, userId);
        logger?.LogDebug("Found {Count} budgets to evaluate for user {UserId}", budgets.Count, userId);

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

          logger?.LogDebug("Budget {BudgetId} evaluation: spent={Spent} amount={Amount}", b.BudgetId, spent, b.Amount);

          if (b.Amount <= 0)
            continue;

          var percent = (int)Math.Floor((spent / b.Amount) * 100);
          var percentBefore = (int)Math.Floor((spentBefore / b.Amount) * 100);

          // check thresholds 90 and 100
          var highestSent = _sentNotifications.GetValueOrDefault(b.BudgetId, 0);

          // Only send 90% notification if we haven't already hit 100% in this transaction
          if (percent >= 90 && percent < 100 && percentBefore < 90 && highestSent < 90)
          {
            // send 90% notification
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
                await emailService.SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
                logger?.LogInformation("Sent 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
              }
              catch (Exception ex)
              {
                logger?.LogWarning(ex, "Failed to send 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
              }
            }
            _sentNotifications.AddOrUpdate(b.BudgetId, 90, (_, __) => 90);
          }

          if (percent >= 100 && percentBefore < 100 && highestSent < 100)
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
                await emailService.SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
                logger?.LogInformation("Sent 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
              }
              catch (Exception ex)
              {
                logger?.LogWarning(ex, "Failed to send 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
              }
            }
            _sentNotifications.AddOrUpdate(b.BudgetId, 100, (_, __) => 100);
          }
        }
      }
      catch (Exception ex)
      {
        logger?.LogError(ex, "Error while evaluating budgets for notifications.");
      }
    }
  }
}