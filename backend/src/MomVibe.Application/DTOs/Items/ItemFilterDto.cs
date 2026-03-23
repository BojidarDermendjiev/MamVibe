namespace MomVibe.Application.DTOs.Items;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;

public class ItemFilterDto
{
    public Guid? CategoryId { get; set; }
    public ListingType? ListingType { get; set; }

    [MaxLength(200)]
    public string? SearchTerm { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    public AgeGroup? AgeGroup { get; set; }
    public int? ShoeSize { get; set; }
    public int? ClothingSize { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 12;

    public string SortBy { get; set; } = "newest";
}
