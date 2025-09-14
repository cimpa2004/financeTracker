using System.Security.Cryptography;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.services;
using Microsoft.AspNetCore.Http;

namespace backend.apis;

public static class LoginApi
{
    public record LoginRequest(string UsernameOrEmail, string Password);

    private const int Pbkdf2Iterations = 100_000;

    public static void MapLogin(this WebApplication app)
    {
        app.MapPost("/api/login", async (LoginRequest req, FinancetrackerContext db, JwtService jwt) =>
        {
            if (string.IsNullOrWhiteSpace(req.UsernameOrEmail) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return Results.BadRequest(new { error = "Username/email and password are required." });
            }

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == req.UsernameOrEmail || u.Email == req.UsernameOrEmail);

            if (user == null)
                return UnauthorizedJson("Invalid username or password.");

            if (!VerifyPassword(req.Password, user.Salt, user.PasswordHash))
                return UnauthorizedJson("Invalid username or password.");

            var tokens = jwt.GenerateTokens(user);

            return Results.Ok(new
            {
                user = new { user.UserId, user.Username, user.Email, user.CreatedAt },
                accessToken = tokens.AccessToken,
                accessTokenExpires = tokens.AccessTokenExpires,
                refreshToken = tokens.RefreshToken,
                refreshTokenExpires = tokens.RefreshTokenExpires,
            });
        })
        .WithName("LoginUser");
    }

    private static bool VerifyPassword(string password, string saltBase64, string hashBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var storedHash = Convert.FromBase64String(hashBase64);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }

    private static IResult UnauthorizedJson(string message) =>
        Results.Json(new { error = message }, statusCode: 401);
}
