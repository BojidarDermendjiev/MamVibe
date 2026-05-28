namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using Common;
using Enums;

[Index(nameof(UserId))]
public class SavedSearch : BaseEntity
{
    [Required]
    public required string UserId { get; set; }

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
    public decimal? MaxPrice { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Category? Category { get; set; }
}
