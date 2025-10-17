using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;
using backend.Helpers;

namespace backend.apis;

public static class BudgetsApi
{
  private static readonly string UnauthorizedMessage = "Unauthorized";
  private static readonly string BudgetNotFoundMessage = "Budget not found.";
  private static readonly string CategoryNotFoundMessage = "Category not found.";
  private static readonly string AmountMustBePositive = "Amount must be greater than or equal to 0.";

  public record AddBudgetRequest(Guid? CategoryId, decimal Amount, string? Name, DateTime? StartDate, DateTime? EndDate);
  public record UpdateBudgetRequest(Guid? CategoryId, decimal Amount, string? Name, DateTime? StartDate, DateTime? EndDate);

  public static void MapBudgets(this WebApplication app)
  {
    app.MapPost("/api/budgets", AddBudget)
       .RequireAuthorization()
       .WithName("AddBudget");

    app.MapGet("/api/budgets", GetBudgets)
       .RequireAuthorization()
       .WithName("GetBudgets");

    app.MapGet("/api/budgets/{id}", GetBudgetById)
       .RequireAuthorization()
       .WithName("GetBudgetById");

    app.MapGet("/api/budgets/{id}/status", GetBudgetStatus)
      .RequireAuthorization()
      .WithName("GetBudgetStatus");

    app.MapGet("/api/budgets/status", GetAllBudgetsStatus)
      .RequireAuthorization()
      .WithName("GetAllBudgetsStatus");

    app.MapPut("/api/budgets/{id}", UpdateBudget)
       .RequireAuthorization()
       .WithName("UpdateBudget");

    app.MapDelete("/api/budgets/{id}", DeleteBudget)
       .RequireAuthorization()
       .WithName("DeleteBudget");
  }

  // Use helpers from backend.Helpers.BudgetHelpers

  private static async Task<IResult> AddBudget(AddBudgetRequest req, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    if (req.Amount < 0)
      return Results.BadRequest(new { error = AmountMustBePositive });

    if (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty)
    {
      var exists = await db.Categories.AnyAsync(c => c.CategoryId == req.CategoryId.Value && (c.UserId == userId || c.UserId == null));
      if (!exists)
        return Results.BadRequest(new { error = CategoryNotFoundMessage });
    }

    var budget = new Budget
    {
      BudgetId = Guid.NewGuid(),
      UserId = userId,
      CategoryId = (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty) ? req.CategoryId : null,
      Name = string.IsNullOrWhiteSpace(req.Name) ? null : req.Name.Trim(),
      StartDate = req.StartDate,
      EndDate = req.EndDate,
      Amount = req.Amount,
      CreatedAt = DateTime.UtcNow
    };

    db.Budgets.Add(budget);
    await db.SaveChangesAsync();

    object? category = null;
    if (budget.CategoryId != null)
    {
      // load the navigation property instead of querying Categories separately
      await db.Entry(budget).Reference(b => b.Category).LoadAsync();
      if (budget.Category != null)
      {
        category = new { budget.Category.CategoryId, budget.Category.Name, budget.Category.Icon, budget.Category.Color, budget.Category.Type };
      }
    }

    var (createdStart, createdEnd) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(budget.StartDate, budget.EndDate);

    var created = new
    {
      budget.BudgetId,
      budget.Name,
      budget.Amount,
      StartDate = createdStart,
      EndDate = createdEnd,
      budget.CreatedAt,
      Category = category,
      User = new { UserId = budget.UserId }
    };

    return Results.Created($"/api/budgets/{budget.BudgetId}", created);
  }

  private static async Task<IResult> GetBudgets(FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    var budgetEntities = await db.Budgets
        .Include(b => b.Category)
        .Where(b => b.UserId == userId)
        .OrderBy(b => b.CategoryId)
        .ToListAsync();

    var budgets = budgetEntities.Select(b =>
    {
      var (rs, re) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(b.StartDate, b.EndDate);
      return new
      {
        b.BudgetId,
        b.Name,
        b.Amount,
        StartDate = rs,
        EndDate = re,
        b.CreatedAt,
        Category = b.Category == null ? null : new { b.Category.CategoryId, b.Category.Name, b.Category.Icon, b.Category.Color, b.Category.Type }
      };
    }).ToList();

    return Results.Ok(budgets);
  }

  private static async Task<IResult> GetBudgetById(Guid id, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    var budgetEntity = await db.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

    if (budgetEntity == null)
      return Results.NotFound(new { error = BudgetNotFoundMessage });

    object? cat = null;
    if (budgetEntity.Category != null)
    {
      cat = new { budgetEntity.Category.CategoryId, budgetEntity.Category.Name, budgetEntity.Category.Icon, budgetEntity.Category.Color, budgetEntity.Category.Type };
    }

    var (bStart, bEnd) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(budgetEntity.StartDate, budgetEntity.EndDate);
    var budget = new
    {
      budgetEntity.BudgetId,
      budgetEntity.Name,
      budgetEntity.Amount,
      StartDate = bStart,
      EndDate = bEnd,
      budgetEntity.CreatedAt,
      Category = cat
    };

    if (budget == null)
      return Results.NotFound(new { error = BudgetNotFoundMessage });

    return Results.Ok(budget);
  }

  private static async Task<IResult> UpdateBudget(Guid id, UpdateBudgetRequest req, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    if (req.Amount < 0)
      return Results.BadRequest(new { error = AmountMustBePositive });

    var budget = await db.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

    if (budget == null)
      return Results.NotFound(new { error = BudgetNotFoundMessage });

    // Validate category if provided
    if (!await backend.Helpers.BudgetHelpers.ValidateCategoryExistsAsync(req.CategoryId, db, userId))
      return Results.BadRequest(new { error = CategoryNotFoundMessage });

    // Apply updates
    budget.CategoryId = (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty) ? req.CategoryId : null;
    budget.Name = string.IsNullOrWhiteSpace(req.Name) ? null : req.Name.Trim();
    budget.StartDate = req.StartDate;
    budget.EndDate = req.EndDate;
    budget.Amount = req.Amount;

    await db.SaveChangesAsync();

    var updatedCat = await backend.Helpers.BudgetHelpers.BuildCategoryObjectAsync(budget, db);

    var (uStart, uEnd) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(budget.StartDate, budget.EndDate);

    var response = new
    {
      budget.BudgetId,
      budget.Name,
      budget.Amount,
      StartDate = uStart,
      EndDate = uEnd,
      budget.CreatedAt,
      Category = updatedCat
    };

    return Results.Ok(response);
  }

  private static async Task<IResult> DeleteBudget(Guid id, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    var budget = await db.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

    if (budget == null)
      return Results.NotFound(new { error = BudgetNotFoundMessage });

    db.Budgets.Remove(budget);
    await db.SaveChangesAsync();

    return Results.NoContent();
  }

  private static async Task<IResult> GetBudgetStatus(Guid id, FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);
    var budget = await db.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

    if (budget == null)
      return Results.NotFound(new { error = BudgetNotFoundMessage });

    var start = budget.StartDate ?? DateTime.MinValue;
    var end = budget.EndDate ?? DateTime.MaxValue;

    var transactions = await db.Transactions
      .Include(t => t.Category)
      .Where(t => t.UserId == userId)
      .ToListAsync();

    DateTime GetTxDate(Transaction t) => t.Date ?? t.CreatedAt ?? DateTime.MinValue;

    var relevant = transactions.Where(t =>
      (budget.CategoryId == null || t.CategoryId == budget.CategoryId)
      && GetTxDate(t) >= start
      && GetTxDate(t) <= end
    );

    var spent = relevant
      .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
      .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

    var remaining = budget.Amount - spent;

    object? category = null;
    if (budget.Category != null)
      category = new { budget.Category.CategoryId, budget.Category.Name, budget.Category.Icon, budget.Category.Color, budget.Category.Type };

    var (sNorm, eNorm) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(budget.StartDate, budget.EndDate);

    var resp = new
    {
      budget.BudgetId,
      budget.Name,
      budget.Amount,
      Spent = spent,
      Remaining = remaining,
      StartDate = sNorm,
      EndDate = eNorm,
      CreatedAt = budget.CreatedAt,
      Category = category
    };

    return Results.Ok(resp);
  }

  private static async Task<IResult> GetAllBudgetsStatus(FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);
    var budgets = await db.Budgets.Include(b => b.Category).Where(b => b.UserId == userId).OrderBy(b => b.CategoryId).ToListAsync();

    // load all transactions for the user once and compute per-budget totals in-memory
    var transactions = await db.Transactions.Include(t => t.Category).Where(t => t.UserId == userId).ToListAsync();

    DateTime GetTxDate(Transaction t) => t.Date ?? t.CreatedAt ?? DateTime.MinValue;

    var results = budgets.Select(b =>
    {
      var start = b.StartDate ?? DateTime.MinValue;
      var end = b.EndDate ?? DateTime.MaxValue;

      var relevant = transactions.Where(t =>
        (b.CategoryId == null || t.CategoryId == b.CategoryId)
        && GetTxDate(t) >= start
        && GetTxDate(t) <= end
      );

      var spent = relevant
        .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
        .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

      var remaining = b.Amount - spent;

      object? category = null;
      if (b.Category != null)
        category = new { b.Category.CategoryId, b.Category.Name, b.Category.Icon, b.Category.Color, b.Category.Type };

      var (rs, re) = backend.Helpers.BudgetHelpers.NormalizeRangeForResponse(b.StartDate, b.EndDate);

      return new
      {
        b.BudgetId,
        b.Name,
        b.Amount,
        Spent = spent,
        Remaining = remaining,
        StartDate = rs,
        EndDate = re,
        CreatedAt = b.CreatedAt,
        Category = category,
        CategoryId = b.CategoryId
      };
    })
  .OrderBy(r => r.CreatedAt)
    .ToList();

    return Results.Ok(results);
  }
}
