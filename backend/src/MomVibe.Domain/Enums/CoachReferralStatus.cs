namespace MomVibe.Domain.Enums;

/// <summary>
/// Lifecycle state of a <c>CoachReferral</c> submitted via the public recommend form
/// or a promoter's tracked referral link.
/// </summary>
public enum CoachReferralStatus
{
    /// <summary>Just submitted; awaiting admin triage.</summary>
    Submitted = 0,

    /// <summary>Admin has reached out to the referred business but onboarding is not complete.</summary>
    Contacted = 1,

    /// <summary>Referred business registered a <c>BusinessProfile</c> on the platform.</summary>
    Onboarded = 2,

    /// <summary>Admin rejected the referral (duplicate, spam, ineligible).</summary>
    Rejected = 3
}
