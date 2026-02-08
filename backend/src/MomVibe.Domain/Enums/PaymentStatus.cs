namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment initiated but not yet completed or confirmed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment was successfully processed and funds captured.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment attempt failed (e.g., declined or error during processing).
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment was intentionally canceled before completion.
    /// </summary>
    Cancelled = 3
}
