namespace MomVibe.Application.Interfaces;

/// <summary>
/// Turnstile verification service contract:
/// - VerifyAsync: validates a Cloudflare Turnstile token with the user's IP address.
/// - VerifyTokenAsync: validates a token without an IP (server-side verification).
/// Returns true when Cloudflare confirms the token is valid.
/// </summary>
public interface ITurnstileService
{
    Task<bool> VerifyAsync(string token, string ip);
    Task<bool> VerifyTokenAsync(string token);
}
