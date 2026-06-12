namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// Local representation of the Stripe subscription that powers a <see cref="BusinessProfile"/>.
/// Exactly one subscription per profile (unique index on <see cref="BusinessProfileId"/>).
/// Status mirrors Stripe and is reconciled via webhook events stored in
/// <see cref="BusinessSubscriptionEvent"/>.
/// </summary>
public class BusinessSubscription : BaseEntity
{
    /// <summary>Owning business profile (unique).</summary>
    public Guid BusinessProfileId { get; set; }

    /// <summary>Code of the active <see cref="SubscriptionPlan"/> (e.g., "Trial", "Basic").</summary>
    [Required]
    [MaxLength(50)]
    public required string PlanCode { get; set; }

    /// <summary>Current subscription status mirrored from Stripe.</summary>
    public BusinessSubscriptionStatus Status { get; set; } = BusinessSubscriptionStatus.Incomplete;

    /// <summary>Stripe subscription identifier (sub_...). Null until checkout completes.</summary>
    [MaxLength(64)]
    public string? StripeSubscriptionId { get; set; }

    /// <summary>UTC start of the current billing period.</summary>
    public DateTime? CurrentPeriodStart { get; set; }

    /// <summary>UTC end of the current billing period.</summary>
    public DateTime? CurrentPeriodEnd { get; set; }

    /// <summary>UTC trial-end timestamp (7 days after checkout for the Trial plan).</summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>UTC timestamp when the subscription was canceled (immediate or at-period-end).</summary>
    public DateTime? CanceledAt { get; set; }

    /// <summary>
    /// UTC deadline before the listing auto-hides after a failed invoice. Cleared
    /// when <c>invoice.payment_succeeded</c> reactivates the subscription.
    /// </summary>
    public DateTime? GracePeriodEndsAt { get; set; }

    /// <summary>Navigation to the owning profile.</summary>
    public BusinessProfile BusinessProfile { get; set; } = null!;

    /// <summary>Append-only ledger of Stripe webhook events that mutated this subscription.</summary>
    public ICollection<BusinessSubscriptionEvent> Events { get; set; } = [];
}
