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
    /// <summary>Gets or sets the updated title for the listing, or <c>null</c> to leave unchanged.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the updated description for the listing, or <c>null</c> to leave unchanged.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the updated category identifier, or <c>null</c> to leave unchanged.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Gets or sets the updated listing type, or <c>null</c> to leave unchanged.</summary>
    public ListingType? ListingType { get; set; }

    /// <summary>Gets or sets the updated listing price, or <c>null</c> to leave unchanged.</summary>
    public decimal? Price { get; set; }

    /// <summary>Gets or sets the updated active status, or <c>null</c> to leave unchanged.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Gets or sets the updated target age group, or <c>null</c> to leave unchanged.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets the updated EU shoe size, or <c>null</c> to leave unchanged.</summary>
    public int? ShoeSize { get; set; }

    /// <summary>Gets or sets the updated EU clothing size, or <c>null</c> to leave unchanged.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets a replacement set of photo URLs, or <c>null</c> to leave the photos unchanged.</summary>
    public List<string>? PhotoUrls { get; set; }
}
