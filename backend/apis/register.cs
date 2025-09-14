using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.apis;

public static class RegisterApi
{
    public record RegisterRequest(string Username, string Email, string Password);

    public static void MapRegister(this WebApplication app)
    {
        app.MapPost("/api/register", async (RegisterRequest req, FinancetrackerContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { error = "Username, email and password are required." });

            if (req.Password.Length < 8)
                return Results.BadRequest(new { error = "Password must be at least 8 characters long." });

            var users = db.Users;

            if (await users.AnyAsync(u => u.Username == req.Username))
                return Results.Conflict(new { error = "Username already taken." });

            if (await users.AnyAsync(u => u.Email == req.Email))
                return Results.Conflict(new { error = "Email already registered." });

            const int saltBytes = 16;
            const int hashBytes = 32;
            const int iterations = 100_000;

            var salt = RandomNumberGenerator.GetBytes(saltBytes);
            using var pbkdf2 = new Rfc2898DeriveBytes(req.Password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(hashBytes);

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(hash),
                CreatedAt = DateTime.UtcNow
            };

            users.Add(user);
            await db.SaveChangesAsync();

            return Results.Ok(new { user.UserId, user.Username, user.Email, user.CreatedAt });
        })
        .WithName("RegisterUser");
    }
}