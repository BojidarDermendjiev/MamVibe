namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Econt Express courier API integration.
/// Bound from the "Econt" section in appsettings.json.
/// </summary>
public class EcontSettings
{
    /// <summary>
    /// Econt API username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Econt API password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Econt REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://demo.econt.com/ee/services";
}
