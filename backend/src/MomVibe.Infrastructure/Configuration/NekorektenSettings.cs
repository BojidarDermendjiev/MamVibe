namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Nekorekten.com buyer-reputation API.
/// Bound from the "Nekorekten" section in appsettings.json.
/// </summary>
public class NekorektenSettings
{
    /// <summary>Secret API key used to authenticate requests to the Nekorekten.com API.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL of the Nekorekten.com API endpoint.</summary>
    public string BaseUrl { get; set; } = "https://api.nekorekten.com";
}
