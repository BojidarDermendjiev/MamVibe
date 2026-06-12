namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// Represents a coach or agency operating on the platform's business vertical.
/// Owned 1:1 by an <see cref="ApplicationUser"/> assigned the <c>Business</c> role.
/// A profile may host at most one <see cref="BusinessListing"/> (enforced by a unique index
/// on <see cref="BusinessListing.BusinessProfileId"/>) and one <see cref="BusinessSubscription"/>.
/// </summary>
public class BusinessProfile : BaseEntity
{
    /// <summary>Identifier of the owning user (FK to ApplicationUser.Id). Unique — one profile per user.</summary>
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>
    /// Top-level business category — drives whether the listing renders on <c>/coaches</c> (Coach)
    /// or <c>/venues</c> (VenueAdvertiser). Set at registration and never changed afterward.
    /// </summary>
    public BusinessCategory Category { get; set; } = BusinessCategory.Coach;

    /// <summary>Single instructor vs. multi-instructor organisation.</summary>
    public ProfileKind ProfileKind { get; set; }

    /// <summary>Legal/registered name of the business (private, shown to admins).</summary>
    [Required]
    [MaxLength(200)]
    public required string LegalName { get; set; }

    /// <summary>Public-facing display name shown on listings and to parents.</summary>
    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    /// <summary>Short public bio (≤2000 chars).</summary>
    [MaxLength(2000)]
    public string? Bio { get; set; }

    /// <summary>Public contact email shown on the listing detail page.</summary>
    [Required]
    [MaxLength(254)]
    public required string ContactEmail { get; set; }

    /// <summary>Public contact phone number (E.164 recommended).</summary>
    [MaxLength(32)]
    public string? ContactPhone { get; set; }

    /// <summary>Optional public website URL.</summary>
    [MaxLength(2048)]
    public string? Website { get; set; }

    /// <summary>Primary operating city.</summary>
    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    /// <summary>
    /// Stripe Customer identifier, populated on first successful subscription checkout.
    /// Persists across subscription lifecycles so a returning business reuses the same Stripe Customer.
    /// </summary>
    [MaxLength(64)]
    public string? StripeCustomerId { get; set; }

    /// <summary>SHA-256 hex of the FingerprintJS visitorId captured at registration (never the raw value).</summary>
    [Required]
    [MaxLength(128)]
    public required string DeviceFingerprintHash { get; set; }

    /// <summary>Truncated registration IP (IPv4 /24 or IPv6 /64). GDPR-proportionate.</summary>
    [MaxLength(45)]
    public string? IpAtRegistration { get; set; }

    /// <summary>User-agent header captured at registration.</summary>
    [MaxLength(512)]
    public string? UserAgentAtRegistration { get; set; }

    /// <summary>Current lifecycle state of the profile.</summary>
    public BusinessProfileStatus Status { get; set; } = BusinessProfileStatus.PendingPolicy;

    /// <summary>FK to the most recently accepted <see cref="BusinessPolicyVersion"/>. Null until first accept.</summary>
    public Guid? PolicyVersionAcceptedId { get; set; }

    /// <summary>
    /// Identifier of the admin who manually bypassed the device-fingerprint duplicate check
    /// when registering this profile (allows legitimate shared-household onboarding).
    /// </summary>
    [MaxLength(450)]
    public string? DeviceCheckBypassedByAdminId { get; set; }

    /// <summary>Reason note attached to the device check bypass (admin-only context).</summary>
    [MaxLength(500)]
    public string? DeviceCheckBypassReason { get; set; }

    /// <summary>Navigation to the owning user.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Navigation to the (optional) single listing owned by this profile.</summary>
    public BusinessListing? Listing { get; set; }

    /// <summary>Navigation to the (optional) Stripe-backed subscription owned by this profile.</summary>
    public BusinessSubscription? Subscription { get; set; }

    /// <summary>Append-only record of every policy version this profile has accepted.</summary>
    public ICollection<BusinessPolicyAcceptance> PolicyAcceptances { get; set; } = [];
}
