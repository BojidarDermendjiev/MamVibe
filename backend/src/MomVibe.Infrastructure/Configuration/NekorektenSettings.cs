namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Nekorekten.com buyer-reputation API.
/// Bound from the "Nekorekten" section in appsettings.json.
/// </summary>
public class NekorektenSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.nekorekten.com";
}
