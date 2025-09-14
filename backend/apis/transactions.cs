using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class TransactionsApi
{
    public record AddTransactionRequest(Guid CategoryId, decimal Amount, string? Description, DateTime? Date);

    public static void MapTransactions(this WebApplication app)
    {
        app.MapPost("/api/addTransaction", async (AddTransactionRequest req, FinancetrackerContext db, HttpContext http) =>
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
    }
}