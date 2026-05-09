namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// Payload sent to the AI-powered price suggestion endpoint.
/// The service analyses comparable listed items and returns a suggested price range.
/// </summary>
public class PriceSuggestionRequestDto
{
    /// <summary>Gets or sets the category identifier used to narrow the pool of comparable items.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the title of the item being listed.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the item being listed.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the target child's age group, used to refine comparables.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets the children's clothing size, used to refine comparables.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets the children's shoe size, used to refine comparables.</summary>
    public int? ShoeSize { get; set; }
}
