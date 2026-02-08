namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

/// <summary>
/// Filter options for listing items:
/// - CategoryId: optional category filter.
/// - ListingType: optional listing type (e.g., Sale/Donation).
/// - SearchTerm: optional text query.
/// - Page: 1-based page index (default: 1).
/// - PageSize: items per page (default: 12).
/// - SortBy: sort key (default: "newest").
/// </summary>
public class ItemFilterDto
{
    public Guid? CategoryId { get; set; }
    public ListingType? ListingType { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string SortBy { get; set; } = "newest";
}
