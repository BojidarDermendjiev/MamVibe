namespace MomVibe.WebApi;

/// <summary>
/// Centralised rate limit policy names used in both StartUp.cs (policy definitions)
/// and controller attributes ([EnableRateLimiting(...)]).
/// Using constants prevents typos from causing silent policy mismatches at runtime.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Broad rate limit policy applied globally to all endpoints.</summary>
    public const string Global = "global";
    /// <summary>Stricter rate limit policy applied to authentication endpoints to mitigate brute-force attacks.</summary>
    public const string Auth = "auth";
    /// <summary>Rate limit policy applied to file upload endpoints.</summary>
    public const string Upload = "upload";
    /// <summary>Rate limit policy applied to the e-bill resend endpoint to prevent abuse.</summary>
    public const string EBillResend = "ebill_resend";
    /// <summary>Rate limit policy applied to the AI assistant chat endpoint.</summary>
    public const string Assistant = "assistant";
    /// <summary>Rate limit policy applied to the item view-count increment endpoint: 1 increment per IP per minute per item to prevent artificial inflation.</summary>
    public const string IncrementView = "increment_view";
    /// <summary>Rate limit policy applied to unauthenticated donation endpoints to prevent Stripe API quota abuse.</summary>
    public const string Donation = "donation";
    /// <summary>Tight rate limit on password-reset endpoints (forgot-password / reset-password) to mitigate token-guessing and user-targeted DoS via reset spam.</summary>
    public const string ForgotPassword = "forgot_password";
    /// <summary>Rate limit policy applied to abuse-report submissions: 10 reports per authenticated user per day.</summary>
    public const string ReportSubmit = "report_submit";
    /// <summary>Rate limit policy applied to moderation-appeal submissions: 1 appeal per user per week per moderation event (with fallback 3/week per user).</summary>
    public const string AppealSubmit = "appeal_submit";
    /// <summary>Rate limit policy applied to anonymous coach-referral submissions: 3 per IP per hour.</summary>
    public const string CoachReferralSubmit = "coach_referral_submit";
}
