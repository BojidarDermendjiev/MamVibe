namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Read + acceptance operations for the versioned business-vertical platform policy.
/// </summary>
public interface IBusinessPolicyService
{
    /// <summary>
    /// Returns the current (<c>IsCurrent=true</c>) policy version for the given language.
    /// Falls back to English if no policy exists for the requested language.
    /// </summary>
    Task<BusinessPolicyDto> GetCurrentAsync(string language);

    /// <summary>
    /// Records an immutable <c>BusinessPolicyAcceptance</c> evidence row for the given user
    /// and version. Updates <c>BusinessProfile.PolicyVersionAcceptedId</c> when a profile exists.
    /// Idempotent — accepting the same version twice for the same profile is a no-op.
    /// </summary>
    Task AcceptAsync(string userId, Guid policyVersionId, string? ip, string? userAgent);
}
