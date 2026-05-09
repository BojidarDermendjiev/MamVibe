namespace MomVibe.Application.DTOs.Payments;

/// <summary>
/// Payload used to initiate a Stripe checkout session for purchasing multiple items in a single transaction.
/// </summary>
public class BulkCheckoutRequest
{
    /// <summary>Gets or sets the list of item identifiers to include in the checkout session.</summary>
    public List<Guid> ItemIds { get; set; } = [];
}
