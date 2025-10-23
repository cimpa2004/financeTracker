using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
  }
}