namespace MomVibe.Infrastructure.Services;

using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// Service for JWT operations: generates signed access tokens with user and role claims
/// and creates cryptographically secure refresh tokens.
/// Configured via strongly-typed <see cref="JwtSettings"/> (Secret, Issuer, Audience, expirations)
/// using HMAC-SHA256 symmetric signing.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;

    public TokenService(IOptions<JwtSettings> jwt)
    {
        this._jwt = jwt.Value;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.DisplayName),
            new("ProfileType", user.ProfileType.ToString()),
            new("IsBlocked", user.IsBlocked.ToString().ToLower())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: this._jwt.Issuer,
            audience: this._jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(this._jwt.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
