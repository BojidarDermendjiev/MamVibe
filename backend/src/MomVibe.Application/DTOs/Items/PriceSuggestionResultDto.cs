namespace MomVibe.Application.DTOs.Items;

public class PriceSuggestionResultDto
{
    public decimal? SuggestedPrice { get; set; }
    public decimal? Low { get; set; }
    public decimal? High { get; set; }
    public double Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int ComparableCount { get; set; }
}
