using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class TransactionsApi
{
  // SQL Server datetime safe bounds
  private static readonly DateTime SqlServerMin = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
  private static readonly DateTime SqlServerMax = System.Data.SqlTypes.SqlDateTime.MaxValue.Value;

  private static readonly string UnauthorizedMessage = "Unauthorized";
  private static readonly string TransactionNotFoundMessage = "Transaction not found.";
  private static readonly string CategoryNotFoundMessage = "Category not found.";
  private static readonly string AmountZeroMessage = "Amount must be non-zero.";
  private static readonly string CategoryIdRequiredMessage = "CategoryId is required.";
  private static readonly string NameTooLongMessage = "Name cannot exceed 255 characters.";
  private static readonly string DescriptionTooLongMessage = "Description cannot exceed 1000 characters.";
  private static readonly string MissingName = "There must be a name.";

  // added optional SubscriptionId
  public record AddTransactionRequest(Guid CategoryId, decimal Amount, string? Description, DateTime? Date, string? Name, Guid? SubscriptionId);

  public static void MapTransactions(this WebApplication app)
  {
    app.MapPost("/api/transactions", async (AddTransactionRequest req, FinancetrackerContext db, HttpContext http, backend.services.IEmailService? emailService, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) =>
      {

        // get user id from JWT claims
        if (!http.TryGetUserId(out var userId))
          return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

        if (req.CategoryId == Guid.Empty)
          return Results.BadRequest(new { error = CategoryIdRequiredMessage });

        if (req.Amount == 0)
          return Results.BadRequest(new { error = AmountZeroMessage });

        if (string.IsNullOrWhiteSpace(req.Name))
          return Results.BadRequest(new { error = MissingName });

        if (req.Name != null && req.Name.Length > 255)
          return Results.BadRequest(new { error = NameTooLongMessage });

        if (!string.IsNullOrWhiteSpace(req.Description) && req.Description.Length > 1000)
          return Results.BadRequest(new { error = DescriptionTooLongMessage });

        // ensure category exists and belongs to the user or is public (UserId == null)
        var categoryExists = await db.Categories
                .AnyAsync(c => c.CategoryId == req.CategoryId &&
                               (c.UserId == userId || c.UserId == null));

        if (!categoryExists)
          return Results.BadRequest(new { error = CategoryNotFoundMessage });

        // if a subscription id was provided, verify it exists and belongs to this user
        if (req.SubscriptionId.HasValue && req.SubscriptionId.Value != Guid.Empty)
        {
          var subExists = await db.Subscriptions
                  .AnyAsync(s => s.SubscriptionId == req.SubscriptionId.Value && s.UserId == userId);

          if (!subExists)
            return Results.BadRequest(new { error = "Subscription not found or does not belong to user." });
        }

        var transaction = new Transaction
        {
          TransactionId = Guid.NewGuid(),
          UserId = userId,
          CategoryId = req.CategoryId,
          Amount = req.Amount,
          Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
          Name = string.IsNullOrWhiteSpace(req.Name) ? null : req.Name.Trim(),
          Date = req.Date ?? DateTime.UtcNow,
          CreatedAt = DateTime.UtcNow,
          SubscriptionId = req.SubscriptionId // optional
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // after saving, evaluate budgets for this user and send notifications via the email service
        var logger = loggerFactory.CreateLogger("TransactionsApi");
        await MailjetEmailService.TryNotifyBudgetsForTransactionAsync(transaction, db, emailService, logger);

        // return nested objects instead of FK ids
        var created = await db.Transactions
                .Where(t => t.TransactionId == transaction.TransactionId)
                .Select(t => new
                {
                  t.TransactionId,
                  Amount = t.Amount,
                  Description = t.Description,
                  Name = t.Name,
                  Date = t.Date,
                  CreatedAt = t.CreatedAt,
                  Category = db.Categories
                        .Where(c => c.CategoryId == t.CategoryId)
                        .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type })
                        .FirstOrDefault(),
                  User = db.Users
                        .Where(u => u.UserId == t.UserId)
                        .Select(u => new { u.UserId, u.Username, u.Email })
                        .FirstOrDefault(),
                  Subscription = t.SubscriptionId == null ? null :
                        db.Subscriptions
                          .Where(s => s.SubscriptionId == t.SubscriptionId)
                          .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive })
                          .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

        return Results.Created($"/api/transactions/{transaction.TransactionId}", created);
      })
  .RequireAuthorization()
      .WithName("AddTransaction");

    app.MapGet("/api/transactions", async (FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var transactions = await db.Transactions
              .Where(t => t.UserId == userId)
              .OrderByDescending(t => t.Date)
              .Select(t => new
              {
                t.TransactionId,
                Amount = t.Amount,
                Description = t.Description,
                Name = t.Name,
                Date = t.Date,
                CreatedAt = t.CreatedAt,
                Category = db.Categories
                      .Where(c => c.CategoryId == t.CategoryId)
                      .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type })
                      .FirstOrDefault(),
                User = db.Users
                      .Where(u => u.UserId == t.UserId)
                      .Select(u => new { u.UserId, u.Username, u.Email })
                      .FirstOrDefault(),
                Subscription = t.SubscriptionId == null ? null :
                      db.Subscriptions
                        .Where(s => s.SubscriptionId == t.SubscriptionId)
                        .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive })
                        .FirstOrDefault()
              })
              .ToListAsync();

      return Results.Ok(transactions);
    })
    .RequireAuthorization()
    .WithName("GetTransactions");

    app.MapGet("/api/transactions/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var transaction = await db.Transactions
              .Where(t => t.TransactionId == id && t.UserId == userId)
              .Select(t => new
              {
                t.TransactionId,
                Amount = t.Amount,
                Description = t.Description,
                Name = t.Name,
                Date = t.Date,
                CreatedAt = t.CreatedAt,
                Category = db.Categories
                      .Where(c => c.CategoryId == t.CategoryId)
                      .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type })
                      .FirstOrDefault(),
                User = db.Users
                      .Where(u => u.UserId == t.UserId)
                      .Select(u => new { u.UserId, u.Username, u.Email })
                      .FirstOrDefault(),
                Subscription = t.SubscriptionId == null ? null :
                      db.Subscriptions
                        .Where(s => s.SubscriptionId == t.SubscriptionId)
                        .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive })
                        .FirstOrDefault()
              })
              .FirstOrDefaultAsync();

      if (transaction == null)
        return Results.NotFound(new { error = TransactionNotFoundMessage });

      return Results.Ok(transaction);
    })
    .RequireAuthorization()
    .WithName("GetTransactionById");

    app.MapDelete("/api/transactions/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var transaction = await db.Transactions
              .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

      if (transaction == null)
        return Results.NotFound(new { error = TransactionNotFoundMessage });

      db.Transactions.Remove(transaction);
      await db.SaveChangesAsync();

      return Results.NoContent();
    })
    .RequireAuthorization()
    .WithName("DeleteTransaction");

    app.MapPut("/api/transactions/{id}", async (Guid id, AddTransactionRequest req, FinancetrackerContext db, HttpContext http, backend.services.IEmailService? emailService, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) =>
          {
            if (!http.TryGetUserId(out var userId))
              return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

            if (req.CategoryId == Guid.Empty)
              return Results.BadRequest(new { error = CategoryIdRequiredMessage });

            if (req.Amount == 0)
              return Results.BadRequest(new { error = AmountZeroMessage });

            if (string.IsNullOrWhiteSpace(req.Name))
              return Results.BadRequest(new { error = MissingName });

            if (req.Name != null && req.Name.Length > 255)
              return Results.BadRequest(new { error = NameTooLongMessage });

            if (!string.IsNullOrWhiteSpace(req.Description) && req.Description.Length > 1000)
              return Results.BadRequest(new { error = DescriptionTooLongMessage });

            var transaction = await db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

            if (transaction == null)
              return Results.NotFound(new { error = TransactionNotFoundMessage });

            // ensure category exists and belongs to the user or is public (UserId == null)
            var categoryExists = await db.Categories
                .AnyAsync(c => c.CategoryId == req.CategoryId &&
                               (c.UserId == userId || c.UserId == null));

            if (!categoryExists)
              return Results.BadRequest(new { error = CategoryNotFoundMessage });

            // if a subscription id was provided, verify it exists and belongs to this user
            if (req.SubscriptionId.HasValue && req.SubscriptionId.Value != Guid.Empty)
            {
              var subExists = await db.Subscriptions
                  .AnyAsync(s => s.SubscriptionId == req.SubscriptionId.Value && s.UserId == userId);

              if (!subExists)
                return Results.BadRequest(new { error = "Subscription not found or does not belong to user." });
            }

            transaction.CategoryId = req.CategoryId;
            transaction.Amount = req.Amount;
            transaction.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
            transaction.Name = string.IsNullOrWhiteSpace(req.Name) ? null : req.Name.Trim();
            transaction.Date = req.Date ?? transaction.Date;
            transaction.SubscriptionId = req.SubscriptionId; // allow adding/removing link

            await db.SaveChangesAsync();

            // after update, evaluate budgets for this user and send notifications via the email service
            var logger = loggerFactory.CreateLogger("TransactionsApi");
            await MailjetEmailService.TryNotifyBudgetsForTransactionAsync(transaction, db, emailService, logger);

            var response = new
            {
              transaction.TransactionId,
              Amount = transaction.Amount,
              Description = transaction.Description,
              Name = transaction.Name,
              Date = transaction.Date,
              CreatedAt = transaction.CreatedAt,
              Category = await db.Categories.Where(c => c.CategoryId == transaction.CategoryId)
                        .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type }).FirstOrDefaultAsync(),
              User = await db.Users.Where(u => u.UserId == transaction.UserId)
                        .Select(u => new { u.UserId, u.Username, u.Email }).FirstOrDefaultAsync(),
              Subscription = transaction.SubscriptionId == null ? null :
                        await db.Subscriptions.Where(s => s.SubscriptionId == transaction.SubscriptionId)
                            .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive }).FirstOrDefaultAsync()
            };

            return Results.Ok(response);
          })
          .RequireAuthorization()
          .WithName("UpdateTransaction");

    app.MapPut("/api/transactions/{id}/setSubscription", async (Guid id, Guid? subscriptionId, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var transaction = await db.Transactions
              .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

      if (transaction == null)
        return Results.NotFound(new { error = TransactionNotFoundMessage });

      // if a subscription id was provided, verify it exists and belongs to this user
      if (subscriptionId.HasValue && subscriptionId.Value != Guid.Empty)
      {
        var subExists = await db.Subscriptions
                .AnyAsync(s => s.SubscriptionId == subscriptionId.Value && s.UserId == userId);

        if (!subExists)
          return Results.BadRequest(new { error = "Subscription not found or does not belong to user." });
      }

      transaction.SubscriptionId = subscriptionId; // allow adding/removing link

      await db.SaveChangesAsync();

      var response = new
      {
        transaction.TransactionId,
        Amount = transaction.Amount,
        Description = transaction.Description,
        Name = transaction.Name,
        Date = transaction.Date,
        CreatedAt = transaction.CreatedAt,
        Category = await db.Categories.Where(c => c.CategoryId == transaction.CategoryId)
                      .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type }).FirstOrDefaultAsync(),
        User = await db.Users.Where(u => u.UserId == transaction.UserId)
                      .Select(u => new { u.UserId, u.Username, u.Email }).FirstOrDefaultAsync(),
        Subscription = transaction.SubscriptionId == null ? null :
                      await db.Subscriptions.Where(s => s.SubscriptionId == transaction.SubscriptionId)
                          .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive }).FirstOrDefaultAsync()
      };

      return Results.Ok(response);
    }).RequireAuthorization()
    .WithName("SetTransactionSubscription");

    app.MapGet("/api/transactions/last3", async (FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var transactions = await db.Transactions
              .Where(t => t.UserId == userId)
              .OrderByDescending(t => t.Date)
              .ThenByDescending(t => t.CreatedAt)
              .Take(3)
              .Select(t => new
              {
                t.TransactionId,
                Amount = t.Amount,
                Description = t.Description,
                Name = t.Name,
                Date = t.Date,
                CreatedAt = t.CreatedAt,
                Category = db.Categories
                      .Where(c => c.CategoryId == t.CategoryId)
                      .Select(c => new { c.CategoryId, c.Name, c.Icon, c.Color, c.Type })
                      .FirstOrDefault(),
                User = db.Users
                      .Where(u => u.UserId == t.UserId)
                      .Select(u => new { u.UserId, u.Username, u.Email })
                      .FirstOrDefault(),
                Subscription = t.SubscriptionId == null ? null :
                      db.Subscriptions
                        .Where(s => s.SubscriptionId == t.SubscriptionId)
                        .Select(s => new { s.SubscriptionId, s.Name, s.Amount, s.Interval, s.PaymentDate, s.IsActive })
                        .FirstOrDefault()
              })
              .ToListAsync();

      return Results.Ok(transactions);
    })
    .RequireAuthorization()
    .WithName("GetLast3Transactions");
  }


}