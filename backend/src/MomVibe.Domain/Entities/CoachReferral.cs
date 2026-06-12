namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// A recommendation submitted via the public <c>/coaches/recommend</c> form or a tracked
/// promoter referral link. Admins review and contact the referred business; if the business
/// subsequently registers a <see cref="BusinessProfile"/>, the referral is marked
/// <see cref="CoachReferralStatus.Onboarded"/>.
/// </summary>
public class CoachReferral : BaseEntity
{
    /// <summary>Name of the referred coach or agency.</summary>
    [Required]
    [MaxLength(200)]
    public required string BusinessName { get; set; }

    /// <summary>Contact email of the referred business (used for admin outreach).</summary>
    [MaxLength(254)]
    public string? ContactEmail { get; set; }

    /// <summary>Contact phone of the referred business.</summary>
    [MaxLength(32)]
    public string? ContactPhone { get; set; }

    /// <summary>Activity category the referred business offers.</summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>City where the referred business operates.</summary>
    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    /// <summary>Free-text notes from the recommender (≤2000 chars).</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>FK to the logged-in user who submitted the referral (null for anonymous submissions).</summary>
    [MaxLength(450)]
    public string? ReferrerUserId { get; set; }

    /// <summary>
    /// <see cref="PromoterProfile.ReferralCode"/> attached to the submission via <c>?ref=</c>.
    /// Null when the submission did not carry a tracking code.
    /// </summary>
    [MaxLength(16)]
    public string? ReferralCode { get; set; }

    /// <summary>Current lifecycle status of the referral.</summary>
    public CoachReferralStatus Status { get; set; } = CoachReferralStatus.Submitted;

    /// <summary>SHA-256 hex of the submitter's IP /24 — used for anonymous rate limiting and abuse review.</summary>
    [MaxLength(64)]
    public string? IpHash { get; set; }

    /// <summary>Admin note attached when the referral is marked Contacted / Onboarded / Rejected.</summary>
    [MaxLength(1000)]
    public string? AdminNote { get; set; }

    /// <summary>Identifier of the admin who last actioned the referral.</summary>
    [MaxLength(450)]
    public string? ActionedByAdminId { get; set; }

    /// <summary>UTC timestamp of the last status transition.</summary>
    public DateTime? ActionedAt { get; set; }

    /// <summary>Navigation to the optional logged-in referrer.</summary>
    public ApplicationUser? Referrer { get; set; }
}
