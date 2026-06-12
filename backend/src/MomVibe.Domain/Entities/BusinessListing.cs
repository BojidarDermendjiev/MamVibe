namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Enums;

/// <summary>
/// The single public advertisement owned by a <see cref="BusinessProfile"/>. Exactly one
/// listing per profile is enforced by a unique index on <see cref="BusinessProfileId"/>;
/// concurrent creates fail with a uniqueness violation translated to HTTP 409 at the service layer.
/// </summary>
public class BusinessListing : BaseEntity
{
    /// <summary>Owning business profile (unique — one listing per profile).</summary>
    public Guid BusinessProfileId { get; set; }

    /// <summary>Listing title shown to parents (≤150 chars).</summary>
    [Required]
    [MaxLength(150)]
    public required string Title { get; set; }

    /// <summary>Long description / programme details (≤4000 chars).</summary>
    [Required]
    [MaxLength(4000)]
    public required string Description { get; set; }

    /// <summary>Activity category (Swimming, MartialArts, etc.).</summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>Primary city.</summary>
    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    /// <summary>Street address (revealed only after the parent taps "Get directions").</summary>
    [MaxLength(300)]
    public string? AddressLine { get; set; }

    /// <summary>Latitude (decimal 10,7) for the Leaflet map pin.</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Longitude (decimal 10,7) for the Leaflet map pin.</summary>
    public decimal? Longitude { get; set; }

    /// <summary>Minimum recommended child age in months (matches <see cref="ChildFriendlyPlace.AgeFromMonths"/>).</summary>
    public short? AgeFromMonths { get; set; }

    /// <summary>Maximum recommended child age in months.</summary>
    public short? AgeToMonths { get; set; }

    /// <summary>Indicative starting price shown to parents (EUR). Optional.</summary>
    public decimal? PriceFromEur { get; set; }

    /// <summary>Free-text schedule summary (e.g., "Mon/Wed/Fri 17:00–18:00"). Structured slots may be added later.</summary>
    [MaxLength(500)]
    public string? Schedule { get; set; }

    /// <summary>When false, the listing is hidden from public browse (owner-toggle or auto-hide on subscription lapse).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True once an admin has approved this listing for public visibility.
    /// Mirrors the <see cref="ChildFriendlyPlace.IsApproved"/> moderation pattern.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Denormalised tier-driven boost (0/0/50/100) used by the browse sort.
    /// Recomputed when the owning subscription's plan changes.
    /// </summary>
    public int RankBoost { get; set; }

    /// <summary>Cached view counter (source of truth is <see cref="BusinessListingViewEvent"/>).</summary>
    public long ViewCount { get; set; }

    /// <summary>Cached like counter (source of truth is <see cref="BusinessListingLike"/>).</summary>
    public long LikeCount { get; set; }

    /// <summary>Cached comment counter (source of truth is <see cref="BusinessListingComment"/>).</summary>
    public long CommentCount { get; set; }

    /// <summary>Navigation to the owning business profile.</summary>
    public BusinessProfile BusinessProfile { get; set; } = null!;

    /// <summary>Photo gallery (cover photo first; admin-moderated like <see cref="ItemPhoto"/>).</summary>
    public ICollection<BusinessListingPhoto> Photos { get; set; } = [];

    /// <summary>Parent likes on this listing.</summary>
    public ICollection<BusinessListingLike> Likes { get; set; } = [];

    /// <summary>Parent comments on this listing.</summary>
    public ICollection<BusinessListingComment> Comments { get; set; } = [];
}
