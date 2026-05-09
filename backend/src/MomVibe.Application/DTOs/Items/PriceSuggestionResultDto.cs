namespace MomVibe.Application.DTOs.Items;

/// <summary>
/// Result returned by the AI-powered price suggestion endpoint, containing
/// a recommended price, a confidence range, and an explanatory rationale.
/// </summary>
public class PriceSuggestionResultDto
{
    /// <summary>Gets or sets the single recommended listing price in BGN, or <c>null</c> if no suggestion could be produced.</summary>
    public decimal? SuggestedPrice { get; set; }

    /// <summary>Gets or sets the lower bound of the suggested price range in BGN.</summary>
    public decimal? Low { get; set; }

    /// <summary>Gets or sets the upper bound of the suggested price range in BGN.</summary>
    public decimal? High { get; set; }

    /// <summary>Gets or sets a value between 0 and 1 indicating the model's confidence in the suggestion.</summary>
    public double Confidence { get; set; }

    /// <summary>Gets or sets a human-readable explanation of how the suggested price was derived.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of comparable items used to generate the suggestion.</summary>
    public int ComparableCount { get; set; }
}
