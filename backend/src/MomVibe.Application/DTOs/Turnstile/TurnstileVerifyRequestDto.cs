namespace MomVibe.Application.DTOs.Turnstile;

/// <summary>
/// Request payload for Cloudflare Turnstile verification:
/// - Token: the client-side Turnstile response token to validate server-side.
/// </summary>
public class TurnstileVerifyRequestDto
{
    /// <summary>Gets or sets the client-side Turnstile response token to validate server-side.</summary>
    public required string Token { get; set; }
}
