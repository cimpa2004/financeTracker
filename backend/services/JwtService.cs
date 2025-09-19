using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Models;

namespace backend.services;

public class JwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenMinutes;

    public JwtService(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? "dev_super_secret_change_me";
        _issuer = config["Jwt:Issuer"] ?? "financetracker";
        _audience = config["Jwt:Audience"] ?? "financetracker_client";
        _accessTokenMinutes = int.TryParse(config["Jwt:AccessTokenExpirationMinutes"], out var m) ? m : 15;
        _refreshTokenMinutes = int.TryParse(config["Jwt:RefreshTokenExpirationMinutes"], out var rm) ? rm : 120;
    }

    public record TokenResult(string AccessToken, DateTime AccessTokenExpires, string RefreshToken, DateTime RefreshTokenExpires);

    public TokenResult GenerateTokens(User user)
    {
        var now = DateTime.UtcNow;

        // common signing key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // access token
        var accessClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var accessExp = now.AddMinutes(_accessTokenMinutes);
        var accessJwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: accessClaims,
            notBefore: now,
            expires: accessExp,
            signingCredentials: creds
        );
        var accessToken = new JwtSecurityTokenHandler().WriteToken(accessJwt);

        // refresh token (include typ=refresh)
        var refreshClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("typ", "refresh")
        };

        var refreshExp = now.AddMinutes(_refreshTokenMinutes);
        var refreshJwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: refreshClaims,
            notBefore: now,
            expires: refreshExp,
            signingCredentials: creds
        );
        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshJwt);

        return new TokenResult(accessToken, accessExp, refreshToken, refreshExp);
    }
}