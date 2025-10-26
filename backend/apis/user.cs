using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.services;

namespace backend.apis;

public static class UserApi
{
  public record UpdateUserRequest(string? Username, string? Email, string? Password);

  public static void MapUser(this WebApplication app)
  {
    app.MapGet("/api/user", GetCurrentUser).RequireAuthorization().WithName("GetCurrentUser");
    app.MapPut("/api/user", UpdateCurrentUser).RequireAuthorization().WithName("UpdateCurrentUser");
  }

  private static async Task<IResult> GetCurrentUser(HttpContext http, FinancetrackerContext db)
  {
    var userId = http.GetUserId();
    if (userId == null) return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
    if (user == null) return Results.NotFound();

    return Results.Ok(new
    {
      userId = user.UserId,
      username = user.Username,
      email = user.Email,
      createdAt = user.CreatedAt?.ToString("o"),
      modifiedAt = user.ModifiedAt?.ToString("o"),
    });
  }

  private static async Task<IResult> UpdateCurrentUser(UpdateUserRequest req, HttpContext http, FinancetrackerContext db)
  {
    var userId = http.GetUserId();
    if (userId == null) return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
    if (user == null) return Results.NotFound();

    // validate and sanitize provided username/email
    if (!string.IsNullOrWhiteSpace(req.Username))
    {
      var trimmed = req.Username.Trim();
      if (trimmed.Length < 1 || trimmed.Length > 255)
        return Results.BadRequest(new { error = "Username must be between 1 and 255 characters." });
      if (trimmed != user.Username)
      {
        if (await db.Users.AnyAsync(u => u.Username == trimmed && u.UserId != user.UserId))
          return Results.Conflict(new { error = "Username already taken." });
        user.Username = trimmed;
      }
    }
    if (!string.IsNullOrWhiteSpace(req.Email))
    {
      var trimmedEmail = req.Email.Trim();
      if (trimmedEmail.Length > 255)
        return Results.BadRequest(new { error = "Email cannot exceed 255 characters." });
      // simple email validation
      var emailAttr = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
      if (!emailAttr.IsValid(trimmedEmail))
        return Results.BadRequest(new { error = "Invalid email address." });
      if (trimmedEmail != user.Email)
      {
        if (await db.Users.AnyAsync(u => u.Email == trimmedEmail && u.UserId != user.UserId))
          return Results.Conflict(new { error = "Email already registered." });
        user.Email = trimmedEmail;
      }
    }

    if (!string.IsNullOrWhiteSpace(req.Password))
    {
      // re-hash password with new salt
      const int saltBytes = 16;
      const int hashBytes = 32;
      const int iterations = 100_000;

      var salt = RandomNumberGenerator.GetBytes(saltBytes);
      using var pbkdf2 = new Rfc2898DeriveBytes(req.Password, salt, iterations, HashAlgorithmName.SHA256);
      var hash = pbkdf2.GetBytes(hashBytes);

      user.Salt = Convert.ToBase64String(salt);
      user.PasswordHash = Convert.ToBase64String(hash);
    }

    user.ModifiedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
      userId = user.UserId,
      username = user.Username,
      email = user.Email,
      createdAt = user.CreatedAt?.ToString("o"),
      modifiedAt = user.ModifiedAt?.ToString("o"),
    });
  }
}
