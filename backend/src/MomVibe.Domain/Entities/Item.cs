namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Represents a user-owned listing with title, description, category, listing type,
/// optional price, and engagement metrics.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - EF Core <see cref="CommentAttribute"/> provides descriptive database column comments.
/// - Indexes support common query patterns (by category, user, activity, etc.).
/// </remarks>
[Index(nameof(CategoryId))]
[Index(nameof(UserId))]
[Index(nameof(IsActive))]
[Index(nameof(ListingType))]
[Index(nameof(CreatedAt))]
public class Item : BaseEntity
{
    /// <summary>
    /// Human-readable item title.
    /// </summary>
    [Required]
    [MinLength(ItemConstants.Lengths.TitleMin)]
    [MaxLength(ItemConstants.Lengths.TitleMax)]
    [Comment(ItemConstants.Comments.Title)]
    public required string Title { get; set; }

    /// <summary>
    /// Detailed description of the item.
    /// </summary>
    [Required]
    [MinLength(ItemConstants.Lengths.DescriptionMin)]
    [MaxLength(ItemConstants.Lengths.DescriptionMax)]
    [Comment(ItemConstants.Comments.Description)]
    public required string Description { get; set; }

    /// <summary>
    /// Foreign key referencing the item's category.
    /// </summary>
    [Comment(ItemConstants.Comments.CategoryId)]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Listing type (domain-specific enumeration).
    /// </summary>
    [Comment(ItemConstants.Comments.ListingType)]
    public ListingType ListingType { get; set; }

    /// <summary>
    /// Age group the item is suitable for; null means unspecified.
    /// </summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>
    /// EU shoe size (e.g. 24, 32, 38); null when not applicable.
    /// </summary>
    public int? ShoeSize { get; set; }

    /// <summary>
    /// EU clothing size (e.g. 56, 86, 128); null when not applicable.
    /// </summary>
    public int? ClothingSize { get; set; }

    /// <summary>
    /// Item price in currency units; null if not applicable.
    /// </summary>
    [Precision(18, 2)]
    [Comment(ItemConstants.Comments.Price)]
    public decimal? Price { get; set; }

    /// <summary>
    /// Foreign key referencing the owning user's identifier.
    /// </summary>
    [Required]
    [Comment(ItemConstants.Comments.UserId)]
    public required string UserId { get; set; }

    /// <summary>
    /// Indicates whether the listing is active/visible.
    /// </summary>
    [Comment(ItemConstants.Comments.IsActive)]
    public bool IsActive { get; set; } = ItemConstants.Defaults.IsActive;

    /// <summary>
    /// Total number of views for this item.
    /// </summary>
    [Comment(ItemConstants.Comments.ViewCount)]
    public int ViewCount { get; set; } = ItemConstants.Defaults.ViewCount;

    /// <summary>
    /// Total number of likes for this item.
    /// </summary>
    [Comment(ItemConstants.Comments.LikeCount)]
    public int LikeCount { get; set; } = ItemConstants.Defaults.LikeCount;

    /// <summary>
    /// Category navigation reference.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Owning user navigation reference.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Associated item photos.
    /// </summary>
    public ICollection<ItemPhoto> Photos { get; set; } = [];

    /// <summary>
    /// Likes for this item.
    /// </summary>
    public ICollection<Like> Likes { get; set; } = [];
}