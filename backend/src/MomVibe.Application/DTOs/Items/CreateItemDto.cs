namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// DTO for creating an item listing:
/// - Title: required name of the item.
/// - Description: required details about the item.
/// - CategoryId: target category identifier.
/// - ListingType: e.g., Sale or Donation.
/// - Price: optional; typically required when ListingType is Sale.
/// - PhotoUrls: list of image URLs for the item (empty by default).
/// </summary>
public class CreateItemDto
{
    /// <summary>Gets or sets the human-readable title of the item listing.</summary>
    public required string Title { get; set; }

    /// <summary>Gets or sets the detailed description of the item.</summary>
    public required string Description { get; set; }

    /// <summary>Gets or sets the identifier of the category this item belongs to.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the listing type (e.g., sale or donation).</summary>
    public ListingType ListingType { get; set; }

    /// <summary>Gets or sets the listing price; required and must be positive when <see cref="ListingType"/> is <c>Sell</c>.</summary>
    public decimal? Price { get; set; }

    /// <summary>Gets or sets the target age group for the item, or <c>null</c> if unspecified.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets the EU shoe size, or <c>null</c> when not applicable.</summary>
    public int? ShoeSize { get; set; }

    /// <summary>Gets or sets the EU clothing size, or <c>null</c> when not applicable.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets the list of image URLs for the item; at least one photo is required.</summary>
    public List<string> PhotoUrls { get; set; } = [];
}
