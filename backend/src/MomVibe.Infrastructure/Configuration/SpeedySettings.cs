namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Speedy courier API integration.
/// Bound from the "Speedy" section in appsettings.json.
/// </summary>
public class SpeedySettings
{
    /// <summary>
    /// Speedy API username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Speedy API password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Speedy REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.speedy.bg/v1";
}
