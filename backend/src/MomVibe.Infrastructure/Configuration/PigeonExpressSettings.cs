namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Pigeon Express courier API integration.
/// Bound from the "PigeonExpress" section in appsettings.json.
/// Obtain credentials by registering a business account at pigeonexpress.com.
/// </summary>
public class PigeonExpressSettings
{
    /// <summary>
    /// API key issued by Pigeon Express for your business account.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Pigeon Express REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.pigeonexpress.com/v1";
}
