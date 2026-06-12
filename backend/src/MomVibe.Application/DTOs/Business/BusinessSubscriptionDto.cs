namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>Read projection of a <c>BusinessSubscription</c> shown on the business dashboard.</summary>
public class BusinessSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid BusinessProfileId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanDisplayName { get; set; } = string.Empty;
    public decimal MonthlyPriceEur { get; set; }
    public int RankBoost { get; set; }
    public BusinessSubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public bool HasStripeSubscription { get; set; }
}

/// <summary>Read projection of a <c>SubscriptionPlan</c> shown on the plan-selector page.</summary>
public class SubscriptionPlanDto
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal MonthlyPriceEur { get; set; }
    public int RankBoost { get; set; }
    public int TrialDays { get; set; }
    public string? FeaturesJson { get; set; }
    public int SortOrder { get; set; }

    /// <summary>True when a Stripe Price id is configured for this plan; false ⇒ checkout will be blocked.</summary>
    public bool IsCheckoutEnabled { get; set; }
}

/// <summary>Request body for starting a subscription checkout.</summary>
public class CreateSubscriptionCheckoutRequest
{
    public string PlanCode { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>Request body for creating a Stripe Customer Portal session.</summary>
public class CreateBillingPortalRequest
{
    public string ReturnUrl { get; set; } = string.Empty;
}
