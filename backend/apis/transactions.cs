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

        // after saving, evaluate budgets for this user and send notifications if thresholds crossed
        var logger = loggerFactory.CreateLogger("TransactionsApi");
        await TryNotifyBudgetsForTransactionAsync(transaction, db, emailService, logger);

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

            // after update, evaluate budgets for this user and send notifications if thresholds crossed
            var logger = loggerFactory.CreateLogger("TransactionsApi");
            await TryNotifyBudgetsForTransactionAsync(transaction, db, emailService, logger);

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

  // Simple in-memory dedup to avoid duplicate notifications for the same budget level during app lifetime.
  // Key = budgetId, value = highest level notified (0 = none, 90, 100)
  private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, int> _sentNotifications = new();

  private static async Task TryNotifyBudgetsForTransactionAsync(Transaction tx, FinancetrackerContext db, backend.services.IEmailService? emailService, Microsoft.Extensions.Logging.ILogger? logger)
  {
    try
    {
      if (emailService == null)
        return; // no email configured or not injected

      var userId = tx.UserId;
      if (userId == Guid.Empty)
        return;

      // load budgets for user that either are global (CategoryId == null) or match the tx.CategoryId
      var budgets = await db.Budgets.Include(b => b.Category)
        .Where(b => b.UserId == userId && (b.CategoryId == null || b.CategoryId == tx.CategoryId))
        .ToListAsync();
      logger?.LogDebug("Evaluating {Count} budgets for category {CategoryId} (including global) and user {UserId}", budgets.Count, tx.CategoryId, userId);
      logger?.LogDebug("Found {Count} budgets to evaluate for user {UserId}", budgets.Count, userId);

      const string UnnamedLabel = "unnamed";
      foreach (var b in budgets)
      {
        var start = b.StartDate ?? SqlServerMin;
        var end = b.EndDate ?? SqlServerMax;

        // gather relevant transactions
        var relevant = await db.Transactions.Include(t => t.Category)
      .Where(t => t.UserId == userId
        && (b.CategoryId == null || t.CategoryId == b.CategoryId)
        && ((t.Date ?? t.CreatedAt ?? SqlServerMin) >= start)
        && ((t.Date ?? t.CreatedAt ?? SqlServerMin) <= end))
            .ToListAsync();

        var spent = relevant
          .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
          .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

        // compute spent before this transaction (exclude this tx) so we only notify when crossing thresholds
        var spentBefore = relevant
          .Where(t => t.TransactionId != tx.TransactionId)
          .Where(t => t.Amount < 0m || (t.Category != null && string.Equals(t.Category.Type, "expense", StringComparison.OrdinalIgnoreCase)))
          .Sum(t => t.Amount < 0m ? -t.Amount : t.Amount);

        logger?.LogDebug("Budget {BudgetId} evaluation: spent={Spent} amount={Amount}", b.BudgetId, spent, b.Amount);

        if (b.Amount <= 0)
          continue;

        var percent = (int)Math.Floor((spent / b.Amount) * 100);
        var percentBefore = (int)Math.Floor((spentBefore / b.Amount) * 100);

        // check thresholds 90 and 100
        var highestSent = _sentNotifications.GetValueOrDefault(b.BudgetId, 0);

        // Only send 90% notification if we haven't already hit 100% in this transaction
        if (percent >= 90 && percent < 100 && percentBefore < 90 && highestSent < 90)
        {
          // send 90% notification
          var user = await db.Users.Where(u => u.UserId == b.UserId).FirstOrDefaultAsync();
          if (user != null && !string.IsNullOrWhiteSpace(user.Email))
          {
            var subjectTemplate = "Budget '{{BudgetName}}' is {{Percent}}% used";
            var htmlTemplate = "<p>Hi,</p><p>Your budget '<strong>{{BudgetName}}</strong>' has used <strong>{{Percent}}%</strong> of its allocated amount ({{Spent}} of {{Amount}}).</p><p>Regards,<br/>Trackaroo team</p>";
            var model = new Dictionary<string, object?>
            {
              ["BudgetName"] = b.Name ?? UnnamedLabel,
              ["Percent"] = percent,
              ["Spent"] = (long)Math.Round(spent),
              ["Amount"] = (long)Math.Round(b.Amount)
            };
            try
            {
              await emailService.SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
              logger?.LogInformation("Sent 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
            }
            catch (Exception ex)
            {
              logger?.LogWarning(ex, "Failed to send 90% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
            }
          }
          _sentNotifications.AddOrUpdate(b.BudgetId, 90, (_, __) => 90);
        }

        if (percent >= 100 && percentBefore < 100 && highestSent < 100)
        {
          var user = await db.Users.Where(u => u.UserId == b.UserId).FirstOrDefaultAsync();
          if (user != null && !string.IsNullOrWhiteSpace(user.Email))
          {
            var subjectTemplate = "Budget '{{BudgetName}}' reached 100%";
            var htmlTemplate = "<p>Hi,</p><p>Your budget '<strong>{{BudgetName}}</strong>' has reached or exceeded its allocated amount. Spent: {{Spent}} of {{Amount}}.</p><p>Regards,<br/>Trackaroo team</p>";
            var model = new Dictionary<string, object?>
            {
              ["BudgetName"] = b.Name ?? UnnamedLabel,
              ["Percent"] = percent,
              ["Spent"] = (long)Math.Round(spent),
              ["Amount"] = (long)Math.Round(b.Amount)
            };
            try
            {
              await emailService.SendTemplatedEmailAsync(user.Email, subjectTemplate, htmlTemplate, model);
              logger?.LogInformation("Sent 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
            }
            catch (Exception ex)
            {
              logger?.LogWarning(ex, "Failed to send 100% email for budget {BudgetId} to {Email}", b.BudgetId, user.Email);
            }
          }
          _sentNotifications.AddOrUpdate(b.BudgetId, 100, (_, __) => 100);
        }
      }
    }
    catch (Exception ex)
    {
      logger?.LogError(ex, "Error while evaluating budgets for notifications.");
    }
  }
}