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
    app.MapGet("/api/spent-by-interval/{interval}", GetSpentByInterval)
     .RequireAuthorization()
     .WithName("spent-by-interval");
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

  private static async Task<IResult> GetSpentByInterval(StatisticInterval interval, DateTime? start, DateTime? end, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

    var now = DateTime.UtcNow;

    List<DateTime> periodStarts = new();
    DateTime makeDayStart(DateTime d) => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Local);

    DateTime AlignToPeriodStart(DateTime dt, StatisticInterval itv)
    {
      dt = ToLocalIfUtc(dt);
      return itv switch
      {
        StatisticInterval.Daily => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.Weekly => makeDayStart(dt.Date).AddDays(-(((int)dt.DayOfWeek + 6) % 7)),
        StatisticInterval.Monthly => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.Yearly => new DateTime(dt.Year, 1, 1, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.AllTime => new DateTime(dt.Year, 1, 1, 0, 0, 0, DateTimeKind.Local),
        _ => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local)
      };
    }

    DateTime AdvancePeriod(DateTime dt, StatisticInterval itv)
    {
      return itv switch
      {
        StatisticInterval.Daily => dt.AddDays(1),
        StatisticInterval.Weekly => dt.AddDays(7),
        StatisticInterval.Monthly => dt.AddMonths(1),
        StatisticInterval.Yearly => dt.AddYears(1),
        StatisticInterval.AllTime => dt.AddYears(1),
        _ => dt.AddDays(1)
      };
    }

    if (start.HasValue || end.HasValue)
    {
      var startLocal = start.HasValue ? AlignToPeriodStart(start.Value, interval) : AlignToPeriodStart(now, interval);
      var endRaw = (end ?? now);
      var endLocal = ToLocalIfUtc(endRaw);
      if (endLocal < startLocal) return Results.BadRequest(new { error = "end must be >= start" });

      DateTime endBound;
      if (endRaw.Kind == DateTimeKind.Utc && endRaw.TimeOfDay.Hours >= 23 && endRaw.TimeOfDay.Minutes >= 59)
      {
        var dateOnlyLocal = DateTime.SpecifyKind(endRaw.ToUniversalTime().Date, DateTimeKind.Local);
        endBound = AlignToPeriodStart(dateOnlyLocal, interval);
      }
      else
      {
        endBound = AlignToPeriodStart(endLocal, interval);
      }

      var cur = startLocal;
      while (cur <= endBound)
      {
        periodStarts.Add(cur);
        cur = AdvancePeriod(cur, interval);
      }
    }
    else
    {
      switch (interval)
      {
        case StatisticInterval.Daily:
          for (int i = 29; i >= 0; i--)
          {
            periodStarts.Add(makeDayStart(now.Date.AddDays(-i)));
          }
          break;
        case StatisticInterval.Weekly:
          var thisWeekStart = makeDayStart(now.Date).AddDays(-(((int)now.DayOfWeek + 6) % 7));
          for (int i = 11; i >= 0; i--)
          {
            periodStarts.Add(thisWeekStart.AddDays(-7 * i));
          }
          break;
        case StatisticInterval.Monthly:
          var firstOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);
          for (int i = 11; i >= 0; i--)
          {
            var dt = firstOfThisMonth.AddMonths(-i);
            periodStarts.Add(new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Local));
          }
          break;
        case StatisticInterval.Yearly:
          var startYear = now.Year - 4;
          for (int y = startYear; y <= now.Year; y++)
          {
            periodStarts.Add(new DateTime(y, 1, 1, 0, 0, 0, DateTimeKind.Local));
          }
          break;
        case StatisticInterval.AllTime:
          var years = await db.Transactions
            .Where(t => t.UserId == userId)
            .Select(t => (t.Date ?? t.CreatedAt))
            .Where(dt => dt.HasValue)
            .Select(dt => dt!.Value.ToLocalTime().Year)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync();
          foreach (var y in years)
          {
            periodStarts.Add(new DateTime(y, 1, 1, 0, 0, 0, DateTimeKind.Local));
          }
          break;
        default:
          return Results.BadRequest(new { error = "Unsupported interval" });
      }
    }

    DateTime? earliest = periodStarts.Count > 0 ? periodStarts[0] : null;

    var txQuery = db.Transactions
      .Where(t => t.UserId == userId)
      .Include(t => t.Category)
      .AsQueryable();

    if (earliest.HasValue && interval != StatisticInterval.AllTime)
    {
      txQuery = txQuery.Where(t => (t.Date ?? t.CreatedAt) >= earliest.Value);
    }

    txQuery = txQuery.Where(t => t.Amount < 0m || (t.Category != null && t.Category.Type == "expense"));

    var txs = await txQuery.ToListAsync();

    DateTime GetPeriodStart(DateTime dt)
    {
      dt = ToLocalIfUtc(dt);
      return interval switch
      {
        StatisticInterval.Daily => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.Weekly => makeDayStart(dt.Date).AddDays(-(((int)dt.DayOfWeek + 6) % 7)),
        StatisticInterval.Monthly => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.Yearly => new DateTime(dt.Year, 1, 1, 0, 0, 0, DateTimeKind.Local),
        StatisticInterval.AllTime => new DateTime(dt.Year, 1, 1, 0, 0, 0, DateTimeKind.Local),
        _ => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local)
      };
    }
    DateTime ToLocalIfUtc(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;



    var grouped = txs
      .Where(t => (t.Date ?? t.CreatedAt).HasValue)
      .GroupBy(t => GetPeriodStart((t.Date ?? t.CreatedAt)!.Value))
      .Select(g => new { PeriodStart = g.Key, Spent = g.Sum(t => t.Amount < 0m ? -t.Amount : t.Amount) })
      .ToDictionary(g => g.PeriodStart, g => g.Spent);

    var results = periodStarts.Select(ps => new { PeriodStart = ps, Spent = grouped.ContainsKey(ps) ? grouped[ps] : 0m }).ToList();

    if (end.HasValue)
    {
      var endLocal = ToLocalIfUtc(end.Value);
      DateTime endBoundFilter;
      if (end.Value.Kind == DateTimeKind.Utc && end.Value.TimeOfDay.Hours >= 23 && end.Value.TimeOfDay.Minutes >= 59)
      {
        var dateOnlyLocal = DateTime.SpecifyKind(end.Value.ToUniversalTime().Date, DateTimeKind.Local);
        endBoundFilter = AlignToPeriodStart(dateOnlyLocal, interval);
      }
      else
      {
        endBoundFilter = AlignToPeriodStart(endLocal, interval);
      }
      results = results.Where(r => r.PeriodStart <= endBoundFilter).ToList();
    }

    return Results.Ok(new { ByPeriod = results });
  }
}