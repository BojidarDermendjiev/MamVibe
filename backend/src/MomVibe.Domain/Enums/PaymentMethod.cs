namespace MomVibe.Domain.Enums;

/// <summary>
/// Identifies the method by which a payment is completed.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Payment completed via card (e.g., through a payment service provider).
    /// </summary>
    Card = 0,
    
    /// <summary>
    /// Payment settled in person at the time of exchange.
    /// </summary>
    OnSpot = 1,

    /// <summary>
    /// Free booking reservation for donated items.
    /// </summary>
    Booking = 2,

    /// <summary>
    /// Payment deducted directly from the buyer's platform wallet balance.
    /// </summary>
    Wallet = 3
}
