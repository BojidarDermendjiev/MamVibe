namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// AI-generated suggestions for prefilling a new item listing form.
/// Returned by the POST /api/items/ai-suggest endpoint.
/// </summary>
public class AiListingSuggestionDto
{
    /// <summary>Gets or sets the AI-suggested title for the item listing.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the AI-suggested description for the item listing.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the category slug to match against the categories list on the frontend.</summary>
    public string CategorySlug { get; set; } = string.Empty;

    /// <summary>Gets or sets the AI-suggested listing type (e.g., sale or donation).</summary>
    public ListingType ListingType { get; set; }

    /// <summary>Gets or sets the AI-suggested price in BGN, or <c>null</c> if the item appears to be a donation.</summary>
    public decimal? SuggestedPrice { get; set; }

    /// <summary>Gets or sets the AI-inferred target age group, or <c>null</c> if not determinable from the photo.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets the AI-inferred EU clothing size, or <c>null</c> if not applicable.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets the AI-inferred EU shoe size, or <c>null</c> if not applicable.</summary>
    public int? ShoeSize { get; set; }
}
