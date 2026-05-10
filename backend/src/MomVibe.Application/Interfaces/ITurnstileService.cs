namespace MomVibe.Application.Interfaces;

/// <summary>
/// Turnstile verification service contract:
/// - VerifyAsync: validates a Cloudflare Turnstile token with the user's IP address.
/// - VerifyTokenAsync: validates a token without an IP (server-side verification).
/// Returns true when Cloudflare confirms the token is valid.
/// </summary>
public interface ITurnstileService
{
    /// <summary>Validates a Cloudflare Turnstile token together with the client's IP address.</summary>
    /// <param name="token">The Turnstile response token from the client.</param>
    /// <param name="ip">The client's IP address.</param>
    /// <returns><c>true</c> if Cloudflare confirms the token is valid.</returns>
    Task<bool> VerifyAsync(string token, string ip);

    /// <summary>Validates a Cloudflare Turnstile token without requiring an IP address (server-side use).</summary>
    /// <param name="token">The Turnstile response token from the client.</param>
    /// <returns><c>true</c> if the token passes verification.</returns>
    Task<bool> VerifyTokenAsync(string token);
}
