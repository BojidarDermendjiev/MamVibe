namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Request payload to refresh JWT credentials:
/// - AccessToken: the (possibly expired) access token.
/// - RefreshToken: the corresponding refresh token to validate and exchange.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>Gets or sets the (possibly expired) JWT access token to validate against the stored token family.</summary>
    public required string AccessToken { get; set; }

    /// <summary>Gets or sets the corresponding refresh token to exchange for a new token pair.</summary>
    public required string RefreshToken { get; set; }
}
