namespace MomVibe.Domain.Enums;

/// <summary>
/// Lifecycle states for a purchase request raised by a buyer.
/// </summary>
public enum PurchaseRequestStatus
{
    /// <summary>Awaiting seller decision.</summary>
    Pending = 0,

    /// <summary>Seller accepted — item stays reserved, buyer completes payment.</summary>
    Accepted = 1,

    /// <summary>Seller declined — item returned to shop.</summary>
    Declined = 2,

    /// <summary>Buyer cancelled before a decision was made.</summary>
    Cancelled = 3,

    /// <summary>Buyer completed payment — transaction fully closed.</summary>
    Completed = 4
}
