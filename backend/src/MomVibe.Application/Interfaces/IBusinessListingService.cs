namespace MomVibe.Application.Interfaces;

using DTOs.Business;
using Domain.Enums;

/// <summary>
/// CRUD for the single <c>BusinessListing</c> owned by a business profile, plus the
/// parents-facing browse + detail reads.
/// </summary>
public interface IBusinessListingService
{
    /// <summary>Returns the public browse result with featured top-slot interleave.
    /// <paramref name="category"/> defaults to <c>Coach</c> when null — venues use a separate browse page.</summary>
    Task<BrowseListingsResult> BrowseAsync(
        BusinessCategory? category,
        string? city,
        ActivityType? activityType,
        int? ageMonths,
        int page,
        int pageSize);

    /// <summary>Returns the full listing detail or null when missing/unapproved.
    /// When <paramref name="currentUserId"/> is provided, the returned DTO carries
    /// <c>IsLikedByCurrentUser</c> populated.</summary>
    Task<BusinessListingDto?> GetByIdAsync(Guid id, string? currentUserId = null, bool includeHidden = false);

    /// <summary>Returns the listing owned by the calling user, or null when they have none yet.</summary>
    Task<BusinessListingDto?> GetMineAsync(string userId);

    /// <summary>Creates the calling user's listing. Throws 409 on duplicate or missing profile.</summary>
    Task<BusinessListingDto> CreateAsync(string userId, CreateBusinessListingRequest request);

    /// <summary>Updates the calling user's listing (or any listing when <paramref name="isAdmin"/>).</summary>
    Task<BusinessListingDto> UpdateAsync(string userId, Guid listingId, UpdateBusinessListingRequest request, bool isAdmin = false);

    /// <summary>Deletes the listing (owner or admin). Photos cascade by FK.</summary>
    Task DeleteAsync(string userId, Guid listingId, bool isAdmin = false);
}
