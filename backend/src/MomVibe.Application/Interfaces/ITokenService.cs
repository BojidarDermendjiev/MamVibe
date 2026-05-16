namespace MomVibe.Application.Interfaces;

using Domain.Entities;

/// <summary>
/// Token service contract for JWT operations:
/// - Generates signed access tokens with user and role claims.
/// - Issues secure refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token for the specified user and roles.</summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <param name="roles">The list of role names to include as claims.</param>
    /// <returns>The signed JWT access token string.</returns>
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);

    /// <summary>Generates a cryptographically secure refresh token string.</summary>
    /// <returns>A new random refresh token.</returns>
    string GenerateRefreshToken();
}
