namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// Append-only ledger of Stripe webhook events that touched a <see cref="BusinessSubscription"/>.
/// <see cref="StripeEventId"/> carries a unique index so replays from Stripe are idempotent —
/// older events are also rejected by comparing <see cref="OccurredAt"/> to the latest applied event.
/// </summary>
public class BusinessSubscriptionEvent : BaseEntity
{
    /// <summary>FK to the subscription this event was applied to.</summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>Canonical Stripe event identifier (<c>evt_...</c>). Unique.</summary>
    [Required]
    [MaxLength(64)]
    public required string StripeEventId { get; set; }

    /// <summary>Categorised event type for fast filtering; raw Stripe type is in <see cref="RawType"/>.</summary>
    public BusinessSubscriptionEventType Type { get; set; }

    /// <summary>Raw Stripe event type string (e.g., <c>customer.subscription.updated</c>).</summary>
    [Required]
    [MaxLength(100)]
    public required string RawType { get; set; }

    /// <summary>JSON payload as received from Stripe (≤32k chars).</summary>
    [Required]
    [MaxLength(32000)]
    public required string PayloadJson { get; set; }

    /// <summary>UTC timestamp reported by Stripe (<c>created</c> field of the event).</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Navigation to the parent subscription.</summary>
    public BusinessSubscription Subscription { get; set; } = null!;
}
