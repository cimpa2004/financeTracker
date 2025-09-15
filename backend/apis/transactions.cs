using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class TransactionsApi
{
    public record AddTransactionRequest(Guid CategoryId, decimal Amount, string? Description, DateTime? Date);

    public static void MapTransactions(this WebApplication app)
    {
        app.MapPost("/api/transactions", async (AddTransactionRequest req, FinancetrackerContext db, HttpContext http) =>
        {

            // get user id from JWT claims
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

            if (req.CategoryId == Guid.Empty)
                return Results.BadRequest(new { error = "CategoryId is required." });

            if (req.Amount == 0)
                return Results.BadRequest(new { error = "Amount must be non-zero." });

            // ensure category exists and belongs to the user or is public (UserId == null)
            var categoryExists = await db.Categories
                .AnyAsync(c => c.CategoryId == req.CategoryId &&
                               (c.UserId == userId || c.UserId == null));

            if (!categoryExists)
                return Results.BadRequest(new { error = "Category not found." });

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = userId,
                CategoryId = req.CategoryId,
                Amount = req.Amount,
                Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
                Date = req.Date ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var response = new
            {
                transaction.TransactionId,
                transaction.UserId,
                transaction.CategoryId,
                transaction.Amount,
                transaction.Description,
                transaction.Date,
                transaction.CreatedAt
            };

            return Results.Created($"/api/transactions/{transaction.TransactionId}", response);
        })
        .RequireAuthorization()
        .WithName("AddTransaction");

        app.MapGet("/api/transactions", async (FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

            var transactions = await db.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.TransactionId,
                    t.UserId,
                    t.CategoryId,
                    t.Amount,
                    t.Description,
                    t.Date,
                    t.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(transactions);
        })
        .RequireAuthorization()
        .WithName("GetTransactions");

        app.MapGet("/api/transactions/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

            var transaction = await db.Transactions
                .Where(t => t.TransactionId == id && t.UserId == userId)
                .Select(t => new
                {
                    t.TransactionId,
                    t.UserId,
                    t.CategoryId,
                    t.Amount,
                    t.Description,
                    t.Date,
                    t.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (transaction == null)
                return Results.NotFound(new { error = "Transaction not found." });

            return Results.Ok(transaction);
        })
        .RequireAuthorization()
        .WithName("GetTransactionById");

        app.MapDelete("/api/transactions/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

            var transaction = await db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

            if (transaction == null)
                return Results.NotFound(new { error = "Transaction not found." });

            db.Transactions.Remove(transaction);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteTransaction");

        app.MapPut("/api/transactions/{id}", async (Guid id, AddTransactionRequest req, FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = "Unauthorized" }, statusCode: 401);

            if (req.CategoryId == Guid.Empty)
                return Results.BadRequest(new { error = "CategoryId is required." });

            if (req.Amount == 0)
                return Results.BadRequest(new { error = "Amount must be non-zero." });

            var transaction = await db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

            if (transaction == null)
                return Results.NotFound(new { error = "Transaction not found." });

            // ensure category exists and belongs to the user or is public (UserId == null)
            var categoryExists = await db.Categories
                .AnyAsync(c => c.CategoryId == req.CategoryId &&
                               (c.UserId == userId || c.UserId == null));

            if (!categoryExists)
                return Results.BadRequest(new { error = "Category not found." });

            transaction.CategoryId = req.CategoryId;
            transaction.Amount = req.Amount;
            transaction.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
            transaction.Date = req.Date ?? transaction.Date;

            await db.SaveChangesAsync();

            var response = new
            {
                transaction.TransactionId,
                transaction.UserId,
                transaction.CategoryId,
                transaction.Amount,
                transaction.Description,
                transaction.Date,
                transaction.CreatedAt
            };

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("UpdateTransaction");
    }
}