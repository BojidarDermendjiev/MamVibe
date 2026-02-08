namespace MomVibe.Infrastructure.Services;

using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

using Application.Interfaces;

/// <summary>
/// Service for verifying Cloudflare Turnstile CAPTCHA tokens via HttpClient.
/// Loads the secret key from configuration (Cloudflare:TurnstileSecretKey), supports verification
/// with token and IP or token-only, auto-passes when using Cloudflare test secrets, and calls the
/// siteverify endpoint to parse success responses.
/// </summary>
public class TurnstileService : ITurnstileService
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;

    public TurnstileService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this._httpClient = httpClientFactory.CreateClient();
        this._secretKey = configuration["Cloudflare:TurnstileSecretKey"]
            ?? throw new InvalidOperationException("Cloudflare:TurnstileSecretKey is not configured.");
    }

    public async Task<bool> VerifyAsync(string token, string ip)
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("secret", this._secretKey),
            new KeyValuePair<string,string>("response", token),
            new KeyValuePair<string,string>("remoteip", ip ?? "")
        });

        var resp = await this._httpClient.PostAsync("turnstile/v0/siteverify", form);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<TurnstileVerifyResponse>();
        return json?.Success == true;
    }
    public async Task<bool> VerifyTokenAsync(string token)
    {
        // Cloudflare test secret keys always pass without calling the API
        if (this._secretKey.StartsWith("1x000000000000000000000000000000"))
            return true;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", this._secretKey },
            { "response", token }
        });

        var response = await this._httpClient.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TurnstileVerifyResponse>(json);

        return result?.Success ?? false;
    }

    private class TurnstileVerifyResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
