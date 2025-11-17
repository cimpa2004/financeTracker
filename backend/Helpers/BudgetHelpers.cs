using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Helpers
{
  public static class BudgetHelpers
  {
    public enum BudgetPeriodType { None, Weekly, Monthly, Yearly }

    // helper to get last day of month (UTC midnight)
    private static DateTime LastDayOfMonth(DateTime d) => new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month), 0, 0, 0, DateTimeKind.Utc);

    // Normalize range for responses (do not persist changes)
    public static (DateTime? start, DateTime? end) NormalizeRangeForResponse(DateTime? startDate, DateTime? endDate)
    {
      DateTime? s = startDate?.Date;
      DateTime? e = endDate?.Date;
      if (s == null && e == null)
        return (null, null);

      // determine ref date
      var now = DateTime.UtcNow.Date;
      var refDate = s ?? e ?? now;

      // if both present, try to detect interval
      if (s.HasValue && e.HasValue)
      {
        var sUtc = DateTime.SpecifyKind(s.Value, DateTimeKind.Utc);
        var eUtc = DateTime.SpecifyKind(e.Value, DateTimeKind.Utc);
        // yearly: starts Jan 1 and ends Dec 31
        if (sUtc.Month == 1 && sUtc.Day == 1 && eUtc.Month == 12 && eUtc.Day == 31)
        {
          var year = sUtc.Year;
          return (new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        }
        // monthly: full month (same month/year)
        if (sUtc.Day == 1 && sUtc.Year == eUtc.Year && sUtc.Month == eUtc.Month && eUtc.Day == DateTime.DaysInMonth(eUtc.Year, eUtc.Month))
        {
          var y = sUtc.Year; var m = sUtc.Month;
          var detectedMonthStart = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
          var detectedMonthEnd = LastDayOfMonth(detectedMonthStart);
          return (detectedMonthStart, detectedMonthEnd);
        }
        // weekly: start Monday and end = start + 6 days
        var startDayOfWeek = (int)sUtc.DayOfWeek; // Sunday=0
        var isMonday = startDayOfWeek == 1;
        if (isMonday && Math.Abs((eUtc - sUtc).TotalDays - 6) < 0.5)
        {
          var st = new DateTime(sUtc.Year, sUtc.Month, sUtc.Day, 0, 0, 0, DateTimeKind.Utc);
          var en = st.AddDays(6);
          return (st, en);
        }
      }

      // fallback: compute monthly range from refDate (first and last day of ref month)
      var refUtc = DateTime.SpecifyKind(refDate, DateTimeKind.Utc);
      var monthStart = new DateTime(refUtc.Year, refUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
      var monthEnd = LastDayOfMonth(monthStart);

      return (monthStart, monthEnd);
    }

    // Detects canonical period type based on exact boundary patterns
    public static BudgetPeriodType DetectPeriodType(DateTime? startDate, DateTime? endDate)
    {
      if (!startDate.HasValue || !endDate.HasValue) return BudgetPeriodType.None;

      var sUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
      var eUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);

      // Yearly: Jan 1 - Dec 31 of same year
      if (sUtc.Month == 1 && sUtc.Day == 1 && eUtc.Month == 12 && eUtc.Day == 31 && sUtc.Year == eUtc.Year)
        return BudgetPeriodType.Yearly;

      // Monthly: 1st to last day of same month
      if (sUtc.Day == 1 && sUtc.Year == eUtc.Year && sUtc.Month == eUtc.Month && eUtc.Day == DateTime.DaysInMonth(eUtc.Year, eUtc.Month))
        return BudgetPeriodType.Monthly;

      // Weekly: Monday .. Sunday (7 days span)
      var isMonday = sUtc.DayOfWeek == DayOfWeek.Monday;
      if (isMonday && (eUtc - sUtc).TotalDays >= 6 && (eUtc - sUtc).TotalDays < 7.1)
        return BudgetPeriodType.Weekly;

      return BudgetPeriodType.None;
    }

    // Advance to next period window for canonical types
    public static (DateTime nextStart, DateTime nextEnd) NextPeriod(DateTime currStart, DateTime currEnd, BudgetPeriodType type)
    {
      currStart = DateTime.SpecifyKind(currStart.Date, DateTimeKind.Utc);
      currEnd = DateTime.SpecifyKind(currEnd.Date, DateTimeKind.Utc);
      switch (type)
      {
        case BudgetPeriodType.Yearly:
          return (new DateTime(currStart.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                  new DateTime(currEnd.Year + 1, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        case BudgetPeriodType.Monthly:
          {
            var nextMonth = currStart.AddMonths(1);
            var start = new DateTime(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = LastDayOfMonth(start);
            return (start, end);
          }
        case BudgetPeriodType.Weekly:
          {
            var start = currStart.AddDays(7);
            var end = start.AddDays(6);
            return (start, end);
          }
        default:
          return (currStart, currEnd);
      }
    }

    public static async Task<bool> ValidateCategoryExistsAsync(Guid? categoryId, FinancetrackerContext db, Guid userId)
    {
      if (!categoryId.HasValue || categoryId.Value == Guid.Empty)
        return true;

      return await db.Categories.AnyAsync(c => c.CategoryId == categoryId.Value && (c.UserId == userId || c.UserId == null));
    }

    public static async Task<object?> BuildCategoryObjectAsync(Budget budget, FinancetrackerContext db)
    {
      if (budget.CategoryId == null)
        return null;

      await db.Entry(budget).Reference(b => b.Category).LoadAsync();
      if (budget.Category == null)
        return null;

      return new { budget.Category.CategoryId, budget.Category.Name, budget.Category.Icon, budget.Category.Color, budget.Category.Type };
    }
  }
}
