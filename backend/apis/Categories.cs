using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class CategoriesApi
{
  private static readonly string UnauthorizedMessage = "Unauthorized";
  private static readonly string CategoryNotFoundMessage = "Category not found.";
  private static readonly string MissingName = "There must be a name.";
  private static readonly string NameTooLongMessage = "Name cannot exceed 255 characters.";
  private static readonly string TypeTooLongMessage = "Type cannot exceed 255 characters.";
  private static readonly string IconTooLongMessage = "Icon cannot exceed 255 characters.";
  private static readonly string ColorTooLongMessage = "Color cannot exceed 50 characters.";
  private static readonly string CannotModifyPublicCategory = "Cannot modify a public category.";

  // allow creating public category by setting IsPublic = true
  public record AddCategoryRequest(string Name, string? Icon, string? Color, string Type, bool IsPublic);

  public static void MapCategories(this WebApplication app)
  {
    app.MapPost("/api/categories", async (AddCategoryRequest req, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = MissingName });

      if (req.Name != null && req.Name.Length > 255)
        return Results.BadRequest(new { error = NameTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Type) && req.Type.Length > 255)
        return Results.BadRequest(new { error = TypeTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Icon) && req.Icon.Length > 255)
        return Results.BadRequest(new { error = IconTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Color) && req.Color.Length > 50)
        return Results.BadRequest(new { error = ColorTooLongMessage });

      var category = new Category
      {
        CategoryId = Guid.NewGuid(),
        Name = req.Name.Trim(),
        Icon = string.IsNullOrWhiteSpace(req.Icon) ? null : req.Icon.Trim(),
        Color = string.IsNullOrWhiteSpace(req.Color) ? null : req.Color.Trim(),
        Type = string.IsNullOrWhiteSpace(req.Type) ? string.Empty : req.Type.Trim(),
        UserId = req.IsPublic ? null : userId
      };

      db.Categories.Add(category);
      await db.SaveChangesAsync();

      var created = new
      {
        category.CategoryId,
        category.Name,
        category.Icon,
        category.Color,
        category.Type,
        IsPublic = category.UserId == null,
        User = category.UserId == null ? null : new { UserId = category.UserId }
      };

      return Results.Created($"/api/categories/{category.CategoryId}", created);
    })
    .RequireAuthorization()
    .WithName("AddCategory");

    app.MapGet("/api/categories", async (FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var categories = await db.Categories
              .Where(c => c.UserId == userId || c.UserId == null)
              .OrderBy(c => c.Name)
              .Select(c => new
              {
                c.CategoryId,
                c.Name,
                c.Icon,
                c.Color,
                c.Type,
                IsPublic = c.UserId == null
              })
              .ToListAsync();

      return Results.Ok(categories);
    })
    .RequireAuthorization()
    .WithName("GetCategories");

    app.MapGet("/api/categories/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var category = await db.Categories
              .Where(c => c.CategoryId == id && (c.UserId == userId || c.UserId == null))
              .Select(c => new
              {
                c.CategoryId,
                c.Name,
                c.Icon,
                c.Color,
                c.Type,
                IsPublic = c.UserId == null
              })
              .FirstOrDefaultAsync();

      if (category == null)
        return Results.NotFound(new { error = CategoryNotFoundMessage });

      return Results.Ok(category);
    })
    .RequireAuthorization()
    .WithName("GetCategoryById");

    app.MapPut("/api/categories/{id}", async (Guid id, AddCategoryRequest req, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = MissingName });

      if (req.Name != null && req.Name.Length > 255)
        return Results.BadRequest(new { error = NameTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Type) && req.Type.Length > 255)
        return Results.BadRequest(new { error = TypeTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Icon) && req.Icon.Length > 255)
        return Results.BadRequest(new { error = IconTooLongMessage });

      if (!string.IsNullOrWhiteSpace(req.Color) && req.Color.Length > 50)
        return Results.BadRequest(new { error = ColorTooLongMessage });

      var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

      if (category == null)
        return Results.NotFound(new { error = CategoryNotFoundMessage });

      // only owner can modify category (public categories cannot be modified)
      if (category.UserId == null || category.UserId != userId)
        return Results.BadRequest(new { error = CannotModifyPublicCategory });

      category.Name = req.Name.Trim();
      category.Icon = string.IsNullOrWhiteSpace(req.Icon) ? null : req.Icon.Trim();
      category.Color = string.IsNullOrWhiteSpace(req.Color) ? null : req.Color.Trim();
      category.Type = string.IsNullOrWhiteSpace(req.Type) ? string.Empty : req.Type.Trim();
      // keep owner as is; disallow making public/private via this endpoint

      await db.SaveChangesAsync();

      var response = new
      {
        category.CategoryId,
        category.Name,
        category.Icon,
        category.Color,
        category.Type,
        IsPublic = category.UserId == null
      };

      return Results.Ok(response);
    })
    .RequireAuthorization()
    .WithName("UpdateCategory");

    app.MapDelete("/api/categories/{id}", async (Guid id, FinancetrackerContext db, HttpContext http) =>
    {
      if (!http.TryGetUserId(out var userId))
        return Results.Json(new { error = UnauthorizedMessage }, statusCode: 401);

      var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

      if (category == null)
        return Results.NotFound(new { error = CategoryNotFoundMessage });

      // only owner can delete
      if (category.UserId == null || category.UserId != userId)
        return Results.BadRequest(new { error = "Cannot delete a public category or a category you do not own." });

      db.Categories.Remove(category);
      await db.SaveChangesAsync();

      return Results.NoContent();
    })
    .RequireAuthorization()
    .WithName("DeleteCategory");
  }
}