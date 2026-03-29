namespace MomVibe.WebApi;

/// <summary>
/// Centralised authorization policy and CORS policy name constants.
/// Prevents typos from causing silent policy mismatches at runtime.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ActiveUser = "ActiveUser";
}

public static class CorsPolicy
{
    public const string MamVibe = "MamVibePolicy";
}
