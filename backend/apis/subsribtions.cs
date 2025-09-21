using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class SubscriptionsApi
{
    private static readonly string UnauthorizedMessage = "Unauthorized";

    public static void MapSubscriptions(this WebApplication app)
    {
        // GET all subscriptions for current user
        app.MapGet("/api/subscriptions", async (FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

            var subs = await db.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.SubscriptionId,
                    s.UserId,
                    s.CategoryId,
                    s.Amount,
                    s.Name,
                    Interval = s.Interval,
                    PaymentDate = s.PaymentDate,
                    s.IsActive,
                    s.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(subs);
        })
        .RequireAuthorization()
        .WithName("GetSubscriptions");

        // GET latest 3 subscriptions for current user (by payment date then created)
        app.MapGet("/api/subscriptions/last3", async (FinancetrackerContext db, HttpContext http) =>
        {
            if (!http.TryGetUserId(out var userId))
                return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

            var subs = await db.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.PaymentDate ?? s.CreatedAt)
                .ThenByDescending(s => s.CreatedAt)
                .Take(3)
                .Select(s => new
                {
                    s.SubscriptionId,
                    s.UserId,
                    s.CategoryId,
                    s.Amount,
                    s.Name,
                    s.Interval,
                    s.PaymentDate,
                    s.IsActive,
                    s.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(subs);
        })
        .RequireAuthorization()
        .WithName("GetLast3Subscriptions");
    }
}