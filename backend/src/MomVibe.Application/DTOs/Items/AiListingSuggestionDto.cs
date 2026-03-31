namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// AI-generated suggestions for prefilling a new item listing form.
/// Returned by the POST /api/items/ai-suggest endpoint.
/// </summary>
public class AiListingSuggestionDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Category slug to match against the categories list on the frontend.</summary>
    public string CategorySlug { get; set; } = string.Empty;
    public ListingType ListingType { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public int? ClothingSize { get; set; }
    public int? ShoeSize { get; set; }
}
