namespace MomVibe.Application.DTOs.SavedSearches;

using Domain.Enums;

public class SavedSearchDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public ListingType? ListingType { get; set; }
    public string? SearchTerm { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public int? ShoeSize { get; set; }
    public int? ClothingSize { get; set; }
    public ItemCondition? Condition { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
