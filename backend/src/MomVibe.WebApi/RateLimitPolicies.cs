namespace MomVibe.WebApi;

/// <summary>
/// Centralised rate limit policy names used in both StartUp.cs (policy definitions)
/// and controller attributes ([EnableRateLimiting(...)]).
/// Using constants prevents typos from causing silent policy mismatches at runtime.
/// </summary>
public static class RateLimitPolicies
{
    public const string Global = "global";
    public const string Auth = "auth";
    public const string Upload = "upload";
}
