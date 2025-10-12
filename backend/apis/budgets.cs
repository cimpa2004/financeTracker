using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class BudgetsApi
{
  private static readonly string UnauthorizedMessage = "Unauthorized";
  private static readonly string BudgetNotFoundMessage = "Budget not found.";
  private static readonly string CategoryNotFoundMessage = "Category not found.";
  private static readonly string AmountMustBePositive = "Amount must be greater than or equal to 0.";

  public record AddBudgetRequest(Guid? CategoryId, decimal Amount);
  public record UpdateBudgetRequest(Guid? CategoryId, decimal Amount);

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

    app.MapPut("/api/budgets/{id}", UpdateBudget)
       .RequireAuthorization()
       .WithName("UpdateBudget");

    app.MapDelete("/api/budgets/{id}", DeleteBudget)
       .RequireAuthorization()
       .WithName("DeleteBudget");
  }

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
      Amount = req.Amount,
      LastReset = DateTime.UtcNow
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

    var created = new
    {
      budget.BudgetId,
      budget.Amount,
      budget.LastReset,
      Category = category,
      User = new { UserId = budget.UserId }
    };

    return Results.Created($"/api/budgets/{budget.BudgetId}", created);
  }

  private static async Task<IResult> GetBudgets(FinancetrackerContext db, HttpContext http)
  {
    if (!http.TryGetUserId(out var userId))
      return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

    var budgets = await db.Budgets
        .Include(b => b.Category)
        .Where(b => b.UserId == userId)
        .OrderBy(b => b.CategoryId)
        .Select(b => new
        {
          b.BudgetId,
          b.Amount,
          b.LastReset,
          Category = b.Category == null ? null : new { b.Category.CategoryId, b.Category.Name, b.Category.Icon, b.Category.Color, b.Category.Type }
        })
        .ToListAsync();

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

    var budget = new
    {
      budgetEntity.BudgetId,
      budgetEntity.Amount,
      budgetEntity.LastReset,
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

    if (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty)
    {
      var exists = await db.Categories.AnyAsync(c => c.CategoryId == req.CategoryId.Value && (c.UserId == userId || c.UserId == null));
      if (!exists)
        return Results.BadRequest(new { error = CategoryNotFoundMessage });
    }

    budget.CategoryId = (req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty) ? req.CategoryId : null;
    budget.Amount = req.Amount;

    await db.SaveChangesAsync();

    object? updatedCat = null;
    if (budget.CategoryId != null)
    {
      await db.Entry(budget).Reference(b => b.Category).LoadAsync();
      if (budget.Category != null)
      {
        updatedCat = new { budget.Category.CategoryId, budget.Category.Name, budget.Category.Icon, budget.Category.Color, budget.Category.Type };
      }
    }

    var response = new
    {
      budget.BudgetId,
      budget.Amount,
      budget.LastReset,
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
}
