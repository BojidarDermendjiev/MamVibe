namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Anthropic Claude API.
/// Bound from the "Anthropic" section in appsettings.json.
/// </summary>
public class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}
