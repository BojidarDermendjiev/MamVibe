namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// Internal DTO returned by <see cref="Interfaces.IAiService.ModerateItemAsync"/>.
/// Not exposed via the API — consumed only by ItemService.
/// </summary>
public class AiModerationResultDto
{
    /// <summary>"approve", "review", or "reject"</summary>
    public string Recommendation { get; set; } = "review";

    /// <summary>Confidence in the recommendation, 0.0–1.0.</summary>
    public double Confidence { get; set; } = 0.5;

    /// <summary>Brief explanation shown to the admin.</summary>
    public string Reason { get; set; } = string.Empty;

    public List<string> Flags { get; set; } = [];
}
