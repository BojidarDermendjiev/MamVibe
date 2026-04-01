namespace MomVibe.Application.DTOs.Items;

using Domain.Enums;

public class PriceSuggestionRequestDto
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AgeGroup? AgeGroup { get; set; }
    public int? ClothingSize { get; set; }
    public int? ShoeSize { get; set; }
}
