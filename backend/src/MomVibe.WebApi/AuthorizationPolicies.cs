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
}

/// <summary>CORS policy name constants used when configuring and applying CORS policies.</summary>
public static class CorsPolicy
{
    /// <summary>The primary CORS policy name for the MamVibe platform.</summary>
    public const string MamVibe = "MamVibePolicy";
}
