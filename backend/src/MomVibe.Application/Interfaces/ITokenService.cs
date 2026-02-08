namespace MomVibe.Application.Interfaces;

using System.Security.Claims;

using Domain.Entities;

/// <summary>
/// Token service contract for JWT operations:
/// - Generates signed access tokens with user and role claims.
/// - Issues secure refresh tokens.
/// - Extracts a ClaimsPrincipal from an expired token without validating lifetime.
/// </summary>
public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
