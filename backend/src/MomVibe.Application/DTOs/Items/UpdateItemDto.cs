namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// DTO for updating an item listing:
/// - Title / Description: optional new values.
/// - CategoryId / ListingType: optionally change categorization and type.
/// - Price: optionally update price.
/// - IsActive: optionally set active status.
/// - PhotoUrls: optionally replace the photos collection.
/// Only provided fields should be applied; unspecified fields remain unchanged.
/// </summary>
public class UpdateItemDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public ListingType? ListingType { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? PhotoUrls { get; set; }
}
