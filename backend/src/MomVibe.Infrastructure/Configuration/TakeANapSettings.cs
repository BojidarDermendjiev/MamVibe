namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for TakeANap courier API integration.
/// Bound from the "TakeANap" section in appsettings.json.
/// </summary>
public class TakeANapSettings
{
    /// <summary>
    /// TakeANap API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// TakeANap API secret for authentication.
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the TakeANap REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.takeanap.bg";

    /// <summary>
    /// Unique shop identifier for TakeANap service.
    /// </summary>
    public string ShopId { get; set; } = string.Empty;
}
