namespace MomVibe.Application.DTOs.SavedSearches;

using Domain.Enums;

/// <summary>
/// Minimal item projection used in saved-search match notifications.
/// Intentionally excludes AI moderation internals (status, notes, score)
/// and raw user IDs to avoid leaking platform signals to end users.
/// </summary>
public class SavedSearchMatchItemDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? CategoryName { get; set; }
    public ListingType ListingType { get; set; }
    public decimal? Price { get; set; }
    public string? FirstPhotoUrl { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public ItemCondition Condition { get; set; }
}
