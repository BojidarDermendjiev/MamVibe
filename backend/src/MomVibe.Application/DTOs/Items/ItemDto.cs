namespace MomVibe.Application.DTOs.Items;

using DTOs.Users;
using Domain.Enums;

/// <summary>
/// DTO representing an item listing:
/// - Identity & content: Id, Title, Description.
/// - Categorization: CategoryId, CategoryName.
/// - Listing details: ListingType (e.g., Sale/Donation), Price (nullable).
/// - Ownership: UserId and optional User profile.
/// - Status & engagement: IsActive, ViewCount, LikeCount, IsLikedByCurrentUser.
/// - Media: Photos collection.
/// - Timestamps: CreatedAt, UpdatedAt.
/// </summary>
public class ItemDto
{
    /// <summary>Gets or sets the unique identifier of the item.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the human-readable title of the item.</summary>
    public required string Title { get; set; }

    /// <summary>Gets or sets the detailed description of the item.</summary>
    public required string Description { get; set; }

    /// <summary>Gets or sets the identifier of the category this item belongs to.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the display name of the category, or <c>null</c> if not loaded.</summary>
    public string? CategoryName { get; set; }

    /// <summary>Gets or sets the listing type (e.g., sale or donation).</summary>
    public ListingType ListingType { get; set; }

    /// <summary>Gets or sets the target age group for the item, or <c>null</c> if unspecified.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets the EU shoe size, or <c>null</c> when not applicable.</summary>
    public int? ShoeSize { get; set; }

    /// <summary>Gets or sets the EU clothing size, or <c>null</c> when not applicable.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets the listing price in currency units, or <c>null</c> for free items.</summary>
    public decimal? Price { get; set; }

    /// <summary>Gets or sets the identifier of the user who owns the listing.</summary>
    public required string UserId { get; set; }

    /// <summary>Gets or sets the display name of the owning user, or <c>null</c> if not loaded.</summary>
    public string? UserDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the owning user, or <c>null</c> if not set.</summary>
    public string? UserAvatarUrl { get; set; }

    /// <summary>Gets or sets the full user profile of the owner, or <c>null</c> if not loaded.</summary>
    public UserDto? User { get; set; }

    /// <summary>Gets or sets a value indicating whether the listing is currently active and publicly visible.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the total number of times this item has been viewed.</summary>
    public int ViewCount { get; set; }

    /// <summary>Gets or sets the total number of likes this item has received.</summary>
    public int LikeCount { get; set; }

    /// <summary>Gets or sets a value indicating whether the current user has liked this item.</summary>
    public bool IsLikedByCurrentUser { get; set; }

    /// <summary>Gets or sets the list of photos associated with this item.</summary>
    public List<ItemPhotoDto> Photos { get; set; } = [];

    /// <summary>Gets or sets the UTC date and time when this item was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the UTC date and time of the most recent update, or <c>null</c> if never updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    // AI Moderation — used in admin views

    /// <summary>Gets or sets the outcome of the AI content moderation run at creation time.</summary>
    public Domain.Enums.AiModerationStatus AiModerationStatus { get; set; }

    /// <summary>Gets or sets the human-readable explanation from the AI moderation model, shown to administrators.</summary>
    public string? AiModerationNotes { get; set; }

    /// <summary>Gets or sets the confidence score returned by the AI moderation model (0.0–1.0).</summary>
    public float? AiModerationScore { get; set; }
}
