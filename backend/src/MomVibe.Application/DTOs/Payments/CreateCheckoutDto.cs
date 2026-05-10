namespace MomVibe.Application.DTOs.Payments;

/// <summary>
/// Request payload to initiate a checkout session:
/// - ItemId: identifier of the item being purchased or donated.
/// </summary>
public class CreateCheckoutDto
{
    /// <summary>Gets or sets the identifier of the item to create a checkout session for.</summary>
    public Guid ItemId { get; set; }
}
