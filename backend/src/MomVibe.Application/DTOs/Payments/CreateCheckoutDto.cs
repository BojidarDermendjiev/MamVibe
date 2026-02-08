namespace MomVibe.Application.DTOs.Payments;

/// <summary>
/// Request payload to initiate a checkout session:
/// - ItemId: identifier of the item being purchased or donated.
/// </summary>
public class CreateCheckoutDto
{
    public Guid ItemId { get; set; }
}
