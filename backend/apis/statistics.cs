using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public enum StatisticInterval
{
  Daily,
  Weekly,
  Monthly,
  Yearly,
  AllTime
}

public static class StatisticsApi
{
  public static void MapStatistics(this WebApplication app)
  {
    app.MapGet("/api/spent-by-category/{interval}", GetSpentByCategory)
       .RequireAuthorization()
       .WithName("spent-by-category");
  }

  private static async Task<IResult> GetSpentByCategory(StatisticInterval interval, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

    var now = DateTime.UtcNow;
    DateTime? periodStart = interval switch
    {
      StatisticInterval.Daily => new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc),
      StatisticInterval.Weekly => new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-(((int)now.DayOfWeek + 6) % 7)),
      StatisticInterval.Monthly => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
      StatisticInterval.Yearly => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
      StatisticInterval.AllTime => null,
      _ => new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc)
    };

    var txQuery = db.Transactions
      .Where(t => t.UserId == userId)
      .Include(t => t.Category)
      .AsQueryable();

    if (periodStart.HasValue)
    {
      txQuery = txQuery.Where(t => (t.Date ?? t.CreatedAt) >= periodStart.Value);
    }

    txQuery = txQuery.Where(t => t.Amount < 0m || (t.Category != null && t.Category.Type == "expense"));

    var grouped = await txQuery
      .GroupBy(t => t.CategoryId)
      .Select(g => new
      {
        CategoryId = g.Key,
        Spent = g.Sum(t => t.Amount < 0m ? -t.Amount : t.Amount)
      })
      .ToListAsync();

    var categoryIds = grouped.Select(g => g.CategoryId).Distinct().ToList();
    var categories = await db.Categories
      .Where(c => categoryIds.Contains(c.CategoryId))
      .Select(c => new { c.CategoryId, Name = (string?)c.Name, Icon = c.Icon, Color = c.Color, Type = (string?)c.Type })
      .ToListAsync();

    var byCategory = grouped.Select(g => new
    {
      Category = categories.FirstOrDefault(c => c.CategoryId == g.CategoryId) ?? new { CategoryId = g.CategoryId, Name = (string?)"Uncategorized", Icon = (string?)null, Color = (string?)null, Type = (string?)null },
      Spent = g.Spent
    }).ToList();

    var total = byCategory.Sum(b => b.Spent);

    return Results.Ok(new { TotalSpent = total, ByCategory = byCategory });
  }
}