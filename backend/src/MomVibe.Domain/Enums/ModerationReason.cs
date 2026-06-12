namespace MomVibe.Domain.Enums;

/// <summary>
/// Categorised reason for a moderation action or abuse report. Used by admins
/// when applying actions, by users when submitting reports, and stored in
/// <c>UserModerationLog.Reason</c> and <c>AbuseReport.Reason</c> for analytics.
/// </summary>
/// <remarks>
/// Distinct from <see cref="ModerationAction"/>, which describes the outcome of
/// item moderation (Approved / Deleted). This enum describes the cause.
/// </remarks>
public enum ModerationReason
{
    /// <summary>Default — reason not yet categorised.</summary>
    Unspecified = 0,
    /// <summary>Repetitive low-value content, mass cross-posting, off-platform solicitation.</summary>
    Spam = 1,
    /// <summary>Fraudulent listings, fake payment proof, advance-fee scams.</summary>
    Scam = 2,
    /// <summary>Targeted abusive messages, threats, hate speech.</summary>
    Harassment = 3,
    /// <summary>Listing of an item the seller does not own or does not exist.</summary>
    FakeListing = 4,
    /// <summary>Photos/text that violate community guidelines (adult, violent, illegal).</summary>
    Inappropriate = 5,
    /// <summary>Stripe chargeback abuse, refund manipulation, payment-method fraud.</summary>
    PaymentFraud = 6,
    /// <summary>Generic terms-of-service violation.</summary>
    RuleViolation = 7,
    /// <summary>Multiple accounts operated by one person to evade prior moderation.</summary>
    MultiAccount = 8,
    /// <summary>Automated activity (bot signup, mass listing, scraping).</summary>
    AutomatedAbuse = 9,
    /// <summary>Auto-detected brute-force login attempt cluster.</summary>
    FailedLoginBurst = 10,
    /// <summary>Admin-applied as a result of a manual review (e.g., escalation from warning).</summary>
    ManualReview = 11,
    /// <summary>An appeal was rejected and the original moderation re-applied.</summary>
    AppealRejected = 12,
    /// <summary>Reason that does not fit any predefined category.</summary>
    Other = 13
}
