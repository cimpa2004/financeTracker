using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class SpentThisMonthApi
{
  private static readonly string UnauthorizedMessage = "Unauthorized";

  public static void MapSpentThisMonth(this WebApplication app)
  {
    app.MapGet("/api/spent-last-month", async (FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var cutoffDate = DateTime.UtcNow.AddMonths(-1);

      // Load them to memory, db side did not work not ideal but works for now
      var transactions = await db.Transactions
        .Include(t => t.Category)
        .Where(t => t.UserId == userId && t.Date >= cutoffDate)
        .ToListAsync();

      var totalSpent = transactions
        .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
        .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

      return Results.Ok(new { spent = totalSpent });
    })
    .RequireAuthorization()
    .WithName("GetSpentLastMonth");
  }
}