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
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required UserDto User { get; set; }
}
