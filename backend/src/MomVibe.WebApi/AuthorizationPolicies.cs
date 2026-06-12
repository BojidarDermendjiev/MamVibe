namespace MomVibe.WebApi;

/// <summary>
/// Centralised authorization policy and CORS policy name constants.
/// Prevents typos from causing silent policy mismatches at runtime.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Policy that restricts access to users in the Admin role.</summary>
    public const string AdminOnly = "AdminOnly";
    /// <summary>Policy that restricts access to authenticated, non-blocked users.</summary>
    public const string ActiveUser = "ActiveUser";
    /// <summary>Policy that gates write actions: blocks Restricted/Suspended/Banned users from creating, updating, or deleting. Reads moderation state from the same distributed cache as <c>UserModerationMiddleware</c>, so worst-case staleness is 60 seconds.</summary>
    public const string WritePermitted = "WritePermitted";
    /// <summary>Policy that restricts access to users in the Business role (coach / agency owners managing their listing and subscription).</summary>
    public const string BusinessOnly = "BusinessOnly";
    /// <summary>Policy that restricts access to users in the Promoter role (referral-code holders managing their referral dashboard).</summary>
    public const string PromoterOnly = "PromoterOnly";
}

/// <summary>CORS policy name constants used when configuring and applying CORS policies.</summary>
public static class CorsPolicy
{
    /// <summary>The primary CORS policy name for the MamVibe platform.</summary>
    public const string MamVibe = "MamVibePolicy";
}
