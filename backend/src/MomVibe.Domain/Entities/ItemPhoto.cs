namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a photo associated with an item, including its URL and display ordering.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validates URL and enforces non-negative display order.
/// - Composite unique index on <c>(ItemId, DisplayOrder)</c> prevents duplicate positions per item.
/// </remarks>
[Index(nameof(ItemId))]
[Index(nameof(ItemId), nameof(DisplayOrder), IsUnique = true)]
public class ItemPhoto : BaseEntity
{
    /// <summary>
    /// URL or relative path to the photo resource.
    /// </summary>
    [Required]
    [MaxLength(ItemPhotoConstants.Lengths.UrlMax)]
    [Comment(ItemPhotoConstants.Comments.Url)]
    public required string Url { get; set; }

    /// <summary>
    /// Foreign key referencing the owning item.
    /// </summary>
    [Comment(ItemPhotoConstants.Comments.ItemId)]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Zero-based display order among the item's photos.
    /// </summary>
    [Range(ItemPhotoConstants.Range.DisplayOrderMin, int.MaxValue)]
    [Comment(ItemPhotoConstants.Comments.DisplayOrder)]
    public int DisplayOrder { get; set; } = ItemPhotoConstants.Defaults.DisplayOrder;

    /// <summary>
    /// Navigation to the owning item.
    /// </summary>
    public Item Item { get; set; } = null!;
}