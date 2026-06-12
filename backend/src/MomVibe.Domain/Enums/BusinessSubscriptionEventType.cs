namespace MomVibe.Domain.Enums;

/// <summary>
/// The subset of Stripe webhook event types the platform persists in
/// <c>BusinessSubscriptionEvent</c> for replay-safe dedup and audit.
/// </summary>
public enum BusinessSubscriptionEventType
{
    /// <summary><c>customer.subscription.created</c></summary>
    SubscriptionCreated = 0,

    /// <summary><c>customer.subscription.updated</c></summary>
    SubscriptionUpdated = 1,

    /// <summary><c>customer.subscription.deleted</c></summary>
    SubscriptionDeleted = 2,

    /// <summary><c>invoice.payment_succeeded</c></summary>
    InvoicePaymentSucceeded = 3,

    /// <summary><c>invoice.payment_failed</c></summary>
    InvoicePaymentFailed = 4,

    /// <summary>Any other Stripe event type forwarded to the subscription webhook handler.</summary>
    Other = 99
}
