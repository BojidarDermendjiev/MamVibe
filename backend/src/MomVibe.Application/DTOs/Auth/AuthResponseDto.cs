namespace MomVibe.Application.DTOs.Auth;

using Users;

/// <summary>
/// Authentication response returned after registration, login, or token refresh:
/// - AccessToken: JWT used for authenticated API requests.
/// - RefreshToken: long-lived token for obtaining new access tokens.
/// - ExpiresAt: timestamp when the access token expires.
/// - User: the authenticated user's profile data.
/// </summary>
public class AuthResponseDto
{
    /// <summary>Gets or sets the JWT access token used to authenticate API requests.</summary>
    public required string AccessToken { get; set; }

    /// <summary>Gets or sets the long-lived refresh token used to obtain new access tokens.</summary>
    public required string RefreshToken { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the access token expires.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets or sets the authenticated user's profile data.</summary>
    public required UserDto User { get; set; }
}
