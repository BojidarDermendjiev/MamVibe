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
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public ListingType ListingType { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public int? ShoeSize { get; set; }
    public int? ClothingSize { get; set; }
    public decimal? Price { get; set; }
    public required string UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public UserDto? User { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<ItemPhotoDto> Photos { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
