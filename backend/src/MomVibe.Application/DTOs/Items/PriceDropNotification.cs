namespace MomVibe.Application.DTOs.Items;

public class PriceDropNotification
{
    public Guid ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string? PhotoUrl { get; set; }
}
