namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Anthropic Claude API.
/// Bound from the "Anthropic" section in appsettings.json.
/// </summary>
public class AnthropicSettings
{
    /// <summary>Secret API key used to authenticate requests to the Anthropic API.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Claude model identifier to use for inference (e.g., claude-haiku-4-5-20251001).</summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}
