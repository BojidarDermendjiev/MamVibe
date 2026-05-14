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
}
