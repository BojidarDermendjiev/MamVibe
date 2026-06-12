namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Owned 1:1 by an <see cref="ApplicationUser"/> assigned the <c>Promoter</c> role.
/// Carries the unique referral code (e.g., <c>MAMA-AB12</c>) used to attribute
/// <see cref="CoachReferral"/> submissions.
/// </summary>
public class PromoterProfile : BaseEntity
{
    /// <summary>Identifier of the owning user (FK to ApplicationUser.Id). Unique — one promoter profile per user.</summary>
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>
    /// Public referral code (8 Base32 chars prefixed <c>MAMA-</c>, e.g., <c>MAMA-AB12CD34</c>).
    /// Unique across the platform.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public required string ReferralCode { get; set; }

    /// <summary>When false, the promoter can still log in but referrals stop accumulating.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Cached count of <see cref="CoachReferral"/> rows attributed to this code.</summary>
    public int TotalReferrals { get; set; }

    /// <summary>Cached count of referrals that reached <see cref="Enums.CoachReferralStatus.Onboarded"/>.</summary>
    public int TotalActivations { get; set; }

    /// <summary>Navigation to the owning user.</summary>
    public ApplicationUser User { get; set; } = null!;
}
