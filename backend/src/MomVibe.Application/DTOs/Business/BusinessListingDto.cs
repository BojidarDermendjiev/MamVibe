namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Full projection of a <c>BusinessListing</c> for the detail page. Includes
/// business contact details, location pin, full photo list, and engagement counters.
/// </summary>
public class BusinessListingDto
{
    public Guid Id { get; set; }
    public Guid BusinessProfileId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public string City { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public short? AgeFromMonths { get; set; }
    public short? AgeToMonths { get; set; }
    public decimal? PriceFromEur { get; set; }
    public string? Schedule { get; set; }

    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public int RankBoost { get; set; }

    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }

    /// <summary>True when the calling user has liked this listing. False for anonymous callers.</summary>
    public bool IsLikedByCurrentUser { get; set; }

    public List<BusinessListingPhotoDto> Photos { get; set; } = [];

    // Business owner public-facing fields.
    public string BusinessDisplayName { get; set; } = string.Empty;
    public string? BusinessBio { get; set; }
    public string BusinessContactEmail { get; set; } = string.Empty;
    public string? BusinessContactPhone { get; set; }
    public string? BusinessWebsite { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Photo projection used inside <see cref="BusinessListingDto.Photos"/>.</summary>
public class BusinessListingPhotoDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCover { get; set; }
}
