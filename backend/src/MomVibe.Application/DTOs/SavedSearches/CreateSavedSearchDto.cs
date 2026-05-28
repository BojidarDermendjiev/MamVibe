namespace MomVibe.Application.DTOs.SavedSearches;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;

public class CreateSavedSearchDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public Guid? CategoryId { get; set; }
    public ListingType? ListingType { get; set; }

    [MaxLength(200)]
    public string? SearchTerm { get; set; }

    public AgeGroup? AgeGroup { get; set; }
    public int? ShoeSize { get; set; }
    public int? ClothingSize { get; set; }
    public ItemCondition? Condition { get; set; }

    [Range(0.01, 999999)]
    public decimal? MaxPrice { get; set; }
}
