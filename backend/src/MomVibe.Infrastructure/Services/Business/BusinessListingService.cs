namespace MomVibe.Infrastructure.Services.Business;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// EF Core-backed CRUD for <see cref="BusinessListing"/> plus the parents-facing browse query.
/// Browse sorts by <c>(RankBoost desc, LikeCount desc, CreatedAt desc)</c>; the top-of-fold
/// "featured" slot is computed as a separate top-2 query and excluded from the main list to
/// avoid double-render.
/// </summary>
public class BusinessListingService : IBusinessListingService
{
    private const int MaxFeatured = 2;
    private const int MaxPhotosPerListing = 15;

    private readonly IApplicationDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ILogger<BusinessListingService> _logger;

    public BusinessListingService(
        IApplicationDbContext db,
        IAuditLogService audit,
        ILogger<BusinessListingService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<BrowseListingsResult> BrowseAsync(
        BusinessCategory? category, string? city, ActivityType? activityType, int? ageMonths, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.BusinessListings
            .AsNoTracking()
            .Include(l => l.BusinessProfile)
            .Include(l => l.Photos)
            .Where(l => l.IsActive && l.IsApproved);

        // Default to Coach when no category is supplied — venues have their own browse page.
        var effectiveCategory = category ?? BusinessCategory.Coach;
        query = query.Where(l => l.BusinessProfile.Category == effectiveCategory);

        if (!string.IsNullOrWhiteSpace(city))
        {
            var loweredCity = city.Trim().ToLower();
            query = query.Where(l => l.City.ToLower().Contains(loweredCity));
        }
        if (activityType.HasValue)
            query = query.Where(l => l.ActivityType == activityType.Value);
        if (ageMonths.HasValue)
        {
            var age = (short)Math.Clamp(ageMonths.Value, 0, 216);
            query = query.Where(l =>
                (l.AgeFromMonths == null || l.AgeFromMonths <= age) &&
                (l.AgeToMonths == null || l.AgeToMonths >= age));
        }

        var totalCount = await query.CountAsync();

        // Featured slot — only computed on page 1 so deep pagination doesn't repeat the cards.
        var featured = new List<BusinessListing>();
        if (page == 1)
        {
            featured = await query
                .Where(l => l.RankBoost > 0)
                .OrderByDescending(l => l.RankBoost)
                .ThenByDescending(l => l.LikeCount)
                .ThenByDescending(l => l.CreatedAt)
                .Take(MaxFeatured)
                .ToListAsync();
        }

        var featuredIds = featured.Select(f => f.Id).ToHashSet();

        var items = await query
            .Where(l => !featuredIds.Contains(l.Id))
            .OrderByDescending(l => l.RankBoost)
            .ThenByDescending(l => l.LikeCount)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new BrowseListingsResult
        {
            Featured = featured.Select(MapToSummary),
            Items = items.Select(MapToSummary),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<BusinessListingDto?> GetByIdAsync(Guid id, string? currentUserId = null, bool includeHidden = false)
    {
        var query = _db.BusinessListings
            .AsNoTracking()
            .Include(l => l.BusinessProfile)
            .Include(l => l.Photos)
            .AsQueryable();
        if (!includeHidden)
            query = query.Where(l => l.IsActive && l.IsApproved);

        var listing = await query.FirstOrDefaultAsync(l => l.Id == id);
        if (listing == null) return null;

        var isLiked = false;
        if (!string.IsNullOrEmpty(currentUserId))
        {
            isLiked = await _db.BusinessListingLikes
                .AsNoTracking()
                .AnyAsync(lk => lk.ListingId == id && lk.UserId == currentUserId);
        }

        return MapToDetail(listing, isLiked);
    }

    public async Task<BusinessListingDto?> GetMineAsync(string userId)
    {
        var profileId = await _db.BusinessProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();
        if (profileId == null) return null;

        var listing = await _db.BusinessListings
            .AsNoTracking()
            .Include(l => l.BusinessProfile)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.BusinessProfileId == profileId.Value);
        return listing == null ? null : MapToDetail(listing, isLikedByCurrentUser: false);
    }

    public async Task<BusinessListingDto> CreateAsync(string userId, CreateBusinessListingRequest request)
    {
        var profile = await _db.BusinessProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new BusinessConflictException("profile_missing",
                "Create your business profile before adding a listing.");

        var alreadyExists = await _db.BusinessListings.AnyAsync(l => l.BusinessProfileId == profile.Id);
        if (alreadyExists)
            throw new BusinessConflictException("listing_already_exists",
                "You already have a listing. Edit or delete it before creating another.");

        if (request.PhotoUrls.Count > MaxPhotosPerListing)
            throw new BusinessConflictException("too_many_photos",
                $"Up to {MaxPhotosPerListing} photos per listing.");

        var listing = new BusinessListing
        {
            BusinessProfileId = profile.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ActivityType = request.ActivityType,
            City = request.City.Trim(),
            AddressLine = string.IsNullOrWhiteSpace(request.AddressLine) ? null : request.AddressLine.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AgeFromMonths = request.AgeFromMonths,
            AgeToMonths = request.AgeToMonths,
            PriceFromEur = request.PriceFromEur,
            Schedule = string.IsNullOrWhiteSpace(request.Schedule) ? null : request.Schedule.Trim(),
            IsActive = true,
            IsApproved = false, // admin moderation
            RankBoost = 0,      // set by subscription tier (Phase 5)
        };
        _db.BusinessListings.Add(listing);

        AppendPhotos(listing, request.PhotoUrls);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Unique-constraint race on BusinessProfileId — translate to clean 409.
            _logger.LogWarning(ex, "BusinessListing create race for profile {ProfileId}", profile.Id);
            throw new BusinessConflictException("listing_already_exists",
                "You already have a listing. Edit or delete it before creating another.");
        }

        await _audit.LogAsync(userId, "Business.Listing.Created", success: true, targetId: listing.Id.ToString());

        // Re-fetch with includes to get the freshly-saved photo rows for the DTO.
        return (await GetByIdAsync(listing.Id, includeHidden: true))!;
    }

    public async Task<BusinessListingDto> UpdateAsync(string userId, Guid listingId, UpdateBusinessListingRequest request, bool isAdmin = false)
    {
        var listing = await _db.BusinessListings
            .Include(l => l.BusinessProfile)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");

        if (!isAdmin && listing.BusinessProfile.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed.");

        if (request.PhotoUrls.Count > MaxPhotosPerListing)
            throw new BusinessConflictException("too_many_photos",
                $"Up to {MaxPhotosPerListing} photos per listing.");

        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.ActivityType = request.ActivityType;
        listing.City = request.City.Trim();
        listing.AddressLine = string.IsNullOrWhiteSpace(request.AddressLine) ? null : request.AddressLine.Trim();
        listing.Latitude = request.Latitude;
        listing.Longitude = request.Longitude;
        listing.AgeFromMonths = request.AgeFromMonths;
        listing.AgeToMonths = request.AgeToMonths;
        listing.PriceFromEur = request.PriceFromEur;
        listing.Schedule = string.IsNullOrWhiteSpace(request.Schedule) ? null : request.Schedule.Trim();
        listing.IsActive = request.IsActive;

        // Replace the photo set in full — simpler than diffing and matches the
        // already-uploaded-URL pattern the client uses.
        foreach (var photo in listing.Photos.ToList())
            _db.BusinessListingPhotos.Remove(photo);
        listing.Photos.Clear();
        AppendPhotos(listing, request.PhotoUrls);

        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId, "Business.Listing.Updated", success: true, targetId: listing.Id.ToString());

        return (await GetByIdAsync(listing.Id, includeHidden: true))!;
    }

    public async Task DeleteAsync(string userId, Guid listingId, bool isAdmin = false)
    {
        var listing = await _db.BusinessListings
            .Include(l => l.BusinessProfile)
            .FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");

        if (!isAdmin && listing.BusinessProfile.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed.");

        _db.BusinessListings.Remove(listing);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId, isAdmin ? "Admin.Business.Listing.Deleted" : "Business.Listing.Deleted",
            success: true, targetId: listingId.ToString());
    }

    private static void AppendPhotos(BusinessListing listing, IEnumerable<string> urls)
    {
        var order = 0;
        foreach (var raw in urls)
        {
            var url = raw?.Trim();
            if (string.IsNullOrEmpty(url)) continue;
            listing.Photos.Add(new BusinessListingPhoto
            {
                Url = url,
                DisplayOrder = order,
                IsCover = order == 0,
            });
            order++;
        }
    }

    private static BusinessListingSummaryDto MapToSummary(BusinessListing l) => new()
    {
        Id = l.Id,
        Title = l.Title,
        ActivityType = l.ActivityType,
        City = l.City,
        AgeFromMonths = l.AgeFromMonths,
        AgeToMonths = l.AgeToMonths,
        PriceFromEur = l.PriceFromEur,
        CoverPhotoUrl = l.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault(),
        BusinessDisplayName = l.BusinessProfile?.DisplayName ?? string.Empty,
        RankBoost = l.RankBoost,
        LikeCount = l.LikeCount,
        CommentCount = l.CommentCount,
        CreatedAt = l.CreatedAt,
    };

    private static BusinessListingDto MapToDetail(BusinessListing l, bool isLikedByCurrentUser) => new()
    {
        Id = l.Id,
        BusinessProfileId = l.BusinessProfileId,
        Title = l.Title,
        Description = l.Description,
        ActivityType = l.ActivityType,
        City = l.City,
        AddressLine = l.AddressLine,
        Latitude = l.Latitude,
        Longitude = l.Longitude,
        AgeFromMonths = l.AgeFromMonths,
        AgeToMonths = l.AgeToMonths,
        PriceFromEur = l.PriceFromEur,
        Schedule = l.Schedule,
        IsActive = l.IsActive,
        IsApproved = l.IsApproved,
        RankBoost = l.RankBoost,
        ViewCount = l.ViewCount,
        LikeCount = l.LikeCount,
        CommentCount = l.CommentCount,
        IsLikedByCurrentUser = isLikedByCurrentUser,
        Photos = l.Photos
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new BusinessListingPhotoDto
            {
                Id = p.Id,
                Url = p.Url,
                DisplayOrder = p.DisplayOrder,
                IsCover = p.IsCover,
            })
            .ToList(),
        BusinessDisplayName = l.BusinessProfile?.DisplayName ?? string.Empty,
        BusinessBio = l.BusinessProfile?.Bio,
        BusinessContactEmail = l.BusinessProfile?.ContactEmail ?? string.Empty,
        BusinessContactPhone = l.BusinessProfile?.ContactPhone,
        BusinessWebsite = l.BusinessProfile?.Website,
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt,
    };
}
