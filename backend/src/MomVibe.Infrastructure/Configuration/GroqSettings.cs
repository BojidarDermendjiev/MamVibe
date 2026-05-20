namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration for the Groq inference API (OpenAI-compatible).
/// Bound from the "Groq" section in appsettings.json / environment variables.
/// </summary>
public class GroqSettings
{
    /// <summary>Secret API key from console.groq.com.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use for chat inference.
    /// Default: llama-3.3-70b-versatile (free tier, fast, excellent quality).
    /// Other options: llama-3.1-8b-instant (faster, lower quality), mixtral-8x7b-32768.
    /// </summary>
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}
