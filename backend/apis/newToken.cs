using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.services;

namespace backend.apis;

public static class NewTokenApi
{
    public record RefreshRequest(string RefreshToken);

    public static void MapNewToken(this WebApplication app)
    {
        app.MapPost("/api/auth/refresh", async (RefreshRequest req, FinancetrackerContext db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(req?.RefreshToken))
                return Results.BadRequest(new { error = "Refresh token is required." });
            System.Console.WriteLine("Refresh token request received.");

            var key = config["Jwt:Key"];
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                return Results.StatusCode(500);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = tokenHandler.ValidateToken(req.RefreshToken, validationParameters, out validatedToken);
            }
            catch
            {
                return Results.Unauthorized();
            }

            // Ensure token is a refresh token
            var typClaim = principal.FindFirst("typ")?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Typ)?.Value;
            if (string.IsNullOrEmpty(typClaim) || !typClaim.Equals("refresh", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst("id")?.Value;

            if (!Guid.TryParse(sub, out var userId))
            {
                return Results.Unauthorized();
            }
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Results.Unauthorized();

            var jwtService = new JwtService(config);
            var tokens = jwtService.GenerateTokens(user);

            // return same shape as login response
            return Results.Ok(new
            {
                user = new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.CreatedAt
                },
                accessToken = tokens.AccessToken,
                accessTokenExpires = tokens.AccessTokenExpires.ToString("o"),
                refreshToken = tokens.RefreshToken,
                refreshTokenExpires = tokens.RefreshTokenExpires.ToString("o")
            });
        })
        .AllowAnonymous();
    }
}