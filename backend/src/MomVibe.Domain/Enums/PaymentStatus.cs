namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a payment transaction.
/// </summary>
/// <remarks>
/// Values 0–3 are the original legacy states (still used for OnSpot / Booking /
/// CashOnDelivery flows that don't go through the Stripe-Connect escrow path).
/// Values 4–8 are the escrow lifecycle introduced in Phase B.2 for online
/// (Card) sales — funds are held on the platform balance until the courier
/// confirms delivery, then released to the seller's Connect account.
/// </remarks>
public enum PaymentStatus
{
    /// <summary>Payment initiated but not yet completed or confirmed.</summary>
    Pending = 0,

    /// <summary>
    /// Legacy non-escrow capture — payment captured directly to the platform with
    /// no held-funds workflow. Retained for OnSpot / Booking / CashOnDelivery
    /// payments and for online sales made before the escrow rollout.
    /// </summary>
    Completed = 1,

    /// <summary>Payment attempt failed (e.g., declined or error during processing).</summary>
    Failed = 2,

    /// <summary>Payment was intentionally canceled before completion.</summary>
    Cancelled = 3,

    /// <summary>
    /// Online sale captured to the platform balance with the seller's net amount
    /// earmarked for transfer to their Connect account after the buyer's 72h
    /// inspection window closes. <see cref="MomVibe.Domain.Entities.Payment.HeldUntil"/>
    /// stores the auto-release deadline.
    /// </summary>
    HeldInEscrow = 4,

    /// <summary>
    /// Escrow funds successfully transferred to the seller's Connect account —
    /// platform retains <see cref="MomVibe.Domain.Entities.Payment.PlatformFeeAmount"/>.
    /// Terminal state for a successful sale.
    /// </summary>
    Released = 5,

    /// <summary>
    /// Buyer fully refunded — both the product amount and the courier fee returned
    /// (used for lost-in-transit cases per the platform's refund matrix).
    /// </summary>
    RefundedFull = 6,

    /// <summary>
    /// Buyer refunded for the product amount only — courier fee retained or
    /// passed to the seller depending on the resolution path
    /// (Returned-by-buyer / Unclaimed).
    /// </summary>
    RefundedProduct = 7,

    /// <summary>
    /// Buyer raised a dispute (return, lost report, etc.) within the inspection
    /// window — release paused pending admin resolution.
    /// </summary>
    Disputed = 8,
}
