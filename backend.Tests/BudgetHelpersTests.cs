using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Helpers;
using backend.Models;
using Xunit;

namespace backend.Tests
{
  public class BudgetHelpersTests
  {
    [Fact]
    public void NormalizeRange_AllTime_ReturnsNulls()
    {
      var (s, e) = BudgetHelpers.NormalizeRangeForResponse(null, null);
      Assert.Null(s);
      Assert.Null(e);
    }

    [Fact]
    public void NormalizeRange_WholeMonth_ReturnsMonthBounds()
    {
      var start = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2025, 3, 31, 0, 0, 0, DateTimeKind.Utc);
      var (s, e) = BudgetHelpers.NormalizeRangeForResponse(start, end);
      Assert.Equal(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), s);
      Assert.Equal(new DateTime(2025, 3, 31, 0, 0, 0, DateTimeKind.Utc), e);
    }

    [Fact]
    public void NormalizeRange_WeekStartingMonday_ReturnsWeekBounds()
    {
      var start = new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Utc); // Monday
      var end = start.AddDays(6);
      var (s, e) = BudgetHelpers.NormalizeRangeForResponse(start, end);
      Assert.Equal(start, s);
      Assert.Equal(end, e);
    }

    [Fact]
    public async Task ValidateCategoryExists_ReturnsTrueWhenMissingOrExists()
    {
      var options = new DbContextOptionsBuilder<FinancetrackerContext>()
        .UseInMemoryDatabase("testdb1")
        .Options;

      using var db = new FinancetrackerContext(options);
      // ensure DB is created
      await db.Database.EnsureDeletedAsync();
      await db.Database.EnsureCreatedAsync();

      var userId = Guid.NewGuid();

      // No category provided -> true
      Assert.True(await BudgetHelpers.ValidateCategoryExistsAsync(null, db, userId));

      // Non-existing category -> false
      var catId = Guid.NewGuid();
      Assert.False(await BudgetHelpers.ValidateCategoryExistsAsync(catId, db, userId));

      // Add category with null UserId (global) -> true
      var cat = new Category { CategoryId = catId, Name = "Global", UserId = null, Type = "expense" };
      db.Categories.Add(cat);
      await db.SaveChangesAsync();

      Assert.True(await BudgetHelpers.ValidateCategoryExistsAsync(catId, db, userId));
    }
  }
}
