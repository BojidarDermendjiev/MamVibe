namespace MomVibe.Application.DTOs.Items;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;

/// <summary>
/// Query parameters used to filter and paginate marketplace item listings.
/// Bind this DTO from query-string values in GET list endpoints.
/// </summary>
public class ItemFilterDto
{
    /// <summary>Gets or sets an optional category identifier to restrict results to a specific category.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Gets or sets an optional listing type filter (e.g., sale, donation).</summary>
    public ListingType? ListingType { get; set; }

    /// <summary>Gets or sets a free-text search term matched against the item title and description.</summary>
    [MaxLength(200)]
    public string? SearchTerm { get; set; }

    /// <summary>Gets or sets an optional brand name filter.</summary>
    [MaxLength(100)]
    public string? Brand { get; set; }

    /// <summary>Gets or sets an optional age group filter for the target child.</summary>
    public AgeGroup? AgeGroup { get; set; }

    /// <summary>Gets or sets an optional children's shoe size filter.</summary>
    public int? ShoeSize { get; set; }

    /// <summary>Gets or sets an optional children's clothing size filter.</summary>
    public int? ClothingSize { get; set; }

    /// <summary>Gets or sets the 1-based page number for pagination. Defaults to 1.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the number of items to return per page. Defaults to 12, maximum 50.</summary>
    [Range(1, 50)]
    public int PageSize { get; set; } = 12;

    /// <summary>Gets or sets the sort order for the results. Accepted values include "newest" and "price_asc". Defaults to "newest".</summary>
    public string SortBy { get; set; } = "newest";
}
