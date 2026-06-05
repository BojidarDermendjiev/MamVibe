namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a photo associated with an item, including its URL and display ordering.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validates URL and enforces non-negative display order.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class ItemPhoto : BaseEntity
{
    /// <summary>
    /// URL or relative path to the photo resource.
    /// </summary>
    [Required]
    [MaxLength(ItemPhotoConstants.Lengths.UrlMax)]
    public required string Url { get; set; }

    /// <summary>
    /// Foreign key referencing the owning item.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Zero-based display order among the item's photos.
    /// </summary>
    [Range(ItemPhotoConstants.Range.DisplayOrderMin, int.MaxValue)]
    public int DisplayOrder { get; set; } = ItemPhotoConstants.Defaults.DisplayOrder;

    /// <summary>
    /// Navigation to the owning item.
    /// </summary>
    public Item Item { get; set; } = null!;
}