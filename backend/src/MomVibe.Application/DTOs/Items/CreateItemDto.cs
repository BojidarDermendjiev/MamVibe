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
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Guid CategoryId { get; set; }
    public ListingType ListingType { get; set; }
    public decimal? Price { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
}
