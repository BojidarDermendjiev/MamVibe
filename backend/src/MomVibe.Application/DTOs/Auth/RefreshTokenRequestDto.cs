namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Request payload to refresh JWT credentials:
/// - AccessToken: the (possibly expired) access token.
/// - RefreshToken: the corresponding refresh token to validate and exchange.
/// </summary>
public class RefreshTokenRequestDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
