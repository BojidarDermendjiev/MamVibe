namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Configuration row describing an available <see cref="BusinessSubscription"/> tier
/// (Trial / Basic / Featured / Premium). Seeded by <c>DataSeeder</c>; admins may
/// adjust pricing or Stripe Price IDs via the admin policies area without redeploys.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Stable string identifier used in API payloads, Stripe metadata, and UI selection
    /// (e.g., "Trial", "Basic", "Featured", "Premium"). Unique.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }

    /// <summary>Display name shown on the plan selector.</summary>
    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    /// <summary>Monthly price in EUR (0 for Trial).</summary>
    public decimal MonthlyPriceEur { get; set; }

    /// <summary>Tier-driven boost added to <see cref="BusinessListing.RankBoost"/> when active.</summary>
    public int RankBoost { get; set; }

    /// <summary>
    /// Number of trial days granted by Stripe via <c>SubscriptionData.TrialPeriodDays</c>
    /// (7 only on the Trial plan; 0 elsewhere).
    /// </summary>
    public int TrialDays { get; set; }

    /// <summary>
    /// Stripe Price ID used by <c>SessionCreateOptions.LineItems</c> when initiating
    /// a subscription checkout. Null when Stripe Prices have not been configured yet.
    /// </summary>
    [MaxLength(120)]
    public string? StripePriceId { get; set; }

    /// <summary>JSON blob of feature flags rendered on the plan card (badges, photo limits, etc.).</summary>
    [MaxLength(2000)]
    public string? FeaturesJson { get; set; }

    /// <summary>When false, the plan is hidden from new sign-ups but existing subscriptions continue.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Display ordering on the plan selector (ascending).</summary>
    public int SortOrder { get; set; }
}
