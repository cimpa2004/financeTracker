using System.Globalization;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class ReportsApi
{
  private static readonly string UnauthorizedMessage = "Unauthorized";

  public static void MapReports(this WebApplication app)
  {
    app.MapGet("/api/reports/budgets", async (FinancetrackerContext db, HttpContext http, ReportService reports) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var fromStr = http.Request.Query["from"].FirstOrDefault();
      var toStr = http.Request.Query["to"].FirstOrDefault();
      DateTime from = DateTime.UtcNow.AddMonths(-1);
      DateTime to = DateTime.UtcNow;
      if (!string.IsNullOrEmpty(fromStr) && DateTime.TryParse(fromStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var f)) from = f;
      if (!string.IsNullOrEmpty(toStr) && DateTime.TryParse(toStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var t)) to = t;

      var pdf = await reports.GenerateBudgetReportPdfAsync(userId, from, to);
      return Results.File(pdf, "application/pdf", $"FinanceReport_{userId}_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
    })
    .RequireAuthorization();
  }
}
