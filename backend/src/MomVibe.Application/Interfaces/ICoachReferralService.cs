namespace MomVibe.Application.Interfaces;

using DTOs.Business;
using Domain.Enums;

/// <summary>
/// Public submission + admin queue for <c>CoachReferral</c>. The public submit path
/// enforces a Turnstile gate (when configured), a 30-day per-contact dedup window,
/// and the rate limit applied at the controller level.
/// </summary>
public interface ICoachReferralService
{
    /// <summary>Submits a public referral. <paramref name="referrerUserId"/> is null for anonymous submissions.</summary>
    Task<Guid> SubmitAsync(SubmitCoachReferralRequest request, string? referrerUserId, string? ipAddress);

    /// <summary>Returns the admin queue, optionally filtered by status.</summary>
    Task<PagedReferralsResult> AdminListAsync(CoachReferralStatus? status, int page, int pageSize);

    /// <summary>Transitions a referral's status (Submitted → Contacted / Onboarded / Rejected).
    /// Promoter counters are updated when the transition is Onboarded.</summary>
    Task UpdateStatusAsync(Guid referralId, UpdateCoachReferralStatusRequest request, string adminId);
}
