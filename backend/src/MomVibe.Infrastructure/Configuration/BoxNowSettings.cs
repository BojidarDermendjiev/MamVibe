namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Box Now courier API integration.
/// Bound from the "BoxNow" section in appsettings.json.
/// </summary>
public class BoxNowSettings
{
    /// <summary>
    /// API key for Box Now authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Box Now REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.boxnow.bg/api/v1";
}
