namespace MomVibe.Infrastructure.Services.Business;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Interfaces;
using Application.DTOs.Business;
using Application.DTOs.Moderation;
using Domain.Entities;
using Domain.Enums;

/// <summary>
/// Likes + comments + report submission for <c>BusinessListing</c>. Reports delegate to
/// <see cref="IAbuseReportService"/> so the existing duplicate-pending + threshold-signal
/// pipeline (and admin queue) covers business listings uniformly with users/items/messages.
/// </summary>
public class BusinessListingInteractionsService : IBusinessListingInteractionsService
{
    private readonly IApplicationDbContext _db;
    private readonly IAbuseReportService _reports;
    private readonly IBusinessRealtimeNotifier _realtime;
    private readonly IAuditLogService _audit;
    private readonly ILogger<BusinessListingInteractionsService> _logger;

    public BusinessListingInteractionsService(
        IApplicationDbContext db,
        IAbuseReportService reports,
        IBusinessRealtimeNotifier realtime,
        IAuditLogService audit,
        ILogger<BusinessListingInteractionsService> logger)
    {
        _db = db;
        _reports = reports;
        _realtime = realtime;
        _audit = audit;
        _logger = logger;
    }

    private async Task<string?> GetListingOwnerAsync(Guid listingId) =>
        await _db.BusinessListings
            .AsNoTracking()
            .Where(l => l.Id == listingId)
            .Join(_db.BusinessProfiles.AsNoTracking(),
                  l => l.BusinessProfileId,
                  p => p.Id,
                  (l, p) => p.UserId)
            .FirstOrDefaultAsync();

    public async Task TrackViewAsync(Guid listingId, string viewerHash)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null) return;
        listing.ViewCount += 1;
        _db.BusinessListingViewEvents.Add(new Domain.Entities.BusinessListingViewEvent
        {
            ListingId = listingId,
            ViewerHash = string.IsNullOrEmpty(viewerHash) ? string.Empty : viewerHash,
            OccurredAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var ownerId = await GetListingOwnerAsync(listingId);
        if (!string.IsNullOrEmpty(ownerId))
        {
            try
            {
                await _realtime.NotifyViewDeltaAsync(ownerId,
                    new ListingViewDelta(listingId, listing.ViewCount));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast view delta for listing {ListingId}", listingId);
            }
        }
    }

    public async Task<ListingLikeStateDto> LikeAsync(string userId, Guid listingId)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");

        var existing = await _db.BusinessListingLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListingId == listingId);

        if (existing == null)
        {
            _db.BusinessListingLikes.Add(new BusinessListingLike
            {
                UserId = userId,
                ListingId = listingId,
            });
            listing.LikeCount += 1;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Composite unique race — another tab beat us. Re-read the count and return.
                _logger.LogDebug("Like race for listing {ListingId} user {UserId}", listingId, userId);
            }
        }

        var newLikeCount = await _db.BusinessListings
            .Where(l => l.Id == listingId)
            .Select(l => l.LikeCount)
            .FirstAsync();

        var ownerId = await GetListingOwnerAsync(listingId);
        if (!string.IsNullOrEmpty(ownerId) && ownerId != userId)
        {
            try
            {
                await _realtime.NotifyLikeDeltaAsync(ownerId,
                    new ListingLikeDelta(listingId, newLikeCount, ActorLiked: true));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast like delta for listing {ListingId}", listingId);
            }
        }

        return new ListingLikeStateDto { IsLiked = true, LikeCount = newLikeCount };
    }

    public async Task<ListingLikeStateDto> UnlikeAsync(string userId, Guid listingId)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");

        var existing = await _db.BusinessListingLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListingId == listingId);
        if (existing != null)
        {
            _db.BusinessListingLikes.Remove(existing);
            if (listing.LikeCount > 0) listing.LikeCount -= 1;
            await _db.SaveChangesAsync();
        }

        var newLikeCount = await _db.BusinessListings
            .Where(l => l.Id == listingId)
            .Select(l => l.LikeCount)
            .FirstAsync();

        var ownerId = await GetListingOwnerAsync(listingId);
        if (!string.IsNullOrEmpty(ownerId) && ownerId != userId)
        {
            try
            {
                await _realtime.NotifyLikeDeltaAsync(ownerId,
                    new ListingLikeDelta(listingId, newLikeCount, ActorLiked: false));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast unlike delta for listing {ListingId}", listingId);
            }
        }

        return new ListingLikeStateDto { IsLiked = false, LikeCount = newLikeCount };
    }

    public async Task<PagedCommentsResult> ListCommentsAsync(Guid listingId, int page, int pageSize, bool includeHidden = false)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var baseQuery = _db.BusinessListingComments
            .AsNoTracking()
            .Where(c => c.ListingId == listingId);
        if (!includeHidden)
            baseQuery = baseQuery.Where(c => !c.IsHidden);

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .Join(_db.Users.AsNoTracking(),
                  c => c.UserId,
                  u => u.Id,
                  (c, u) => new BusinessListingCommentDto
                  {
                      Id = c.Id,
                      ListingId = c.ListingId,
                      UserId = c.UserId,
                      AuthorDisplayName = u.DisplayName,
                      AuthorAvatarUrl = u.AvatarUrl,
                      Body = c.Body,
                      ParentCommentId = c.ParentCommentId,
                      IsHidden = c.IsHidden,
                      HiddenReason = includeHidden ? c.HiddenReason : null,
                      CreatedAt = c.CreatedAt,
                  })
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedCommentsResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<BusinessListingCommentDto> AddCommentAsync(string userId, Guid listingId, CreateBusinessListingCommentRequest request)
    {
        var listing = await _db.BusinessListings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw new KeyNotFoundException("Listing not found.");

        // Single-level threading — replies cannot themselves be replied to.
        if (request.ParentCommentId.HasValue)
        {
            var parent = await _db.BusinessListingComments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value && c.ListingId == listingId)
                ?? throw new KeyNotFoundException("Parent comment not found on this listing.");
            if (parent.ParentCommentId.HasValue)
                throw new InvalidOperationException("Replies cannot be nested further than one level.");
        }

        var comment = new BusinessListingComment
        {
            ListingId = listingId,
            UserId = userId,
            Body = request.Body.Trim(),
            ParentCommentId = request.ParentCommentId,
        };
        _db.BusinessListingComments.Add(comment);
        listing.CommentCount += 1;
        await _db.SaveChangesAsync();

        var author = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.DisplayName, u.AvatarUrl })
            .FirstAsync();

        var dto = new BusinessListingCommentDto
        {
            Id = comment.Id,
            ListingId = comment.ListingId,
            UserId = comment.UserId,
            AuthorDisplayName = author.DisplayName,
            AuthorAvatarUrl = author.AvatarUrl,
            Body = comment.Body,
            ParentCommentId = comment.ParentCommentId,
            IsHidden = false,
            HiddenReason = null,
            CreatedAt = comment.CreatedAt,
        };

        var ownerId = await GetListingOwnerAsync(listingId);
        if (!string.IsNullOrEmpty(ownerId) && ownerId != userId)
        {
            try
            {
                await _realtime.NotifyCommentAsync(ownerId,
                    new ListingCommentBroadcast(listingId, dto, Deleted: false));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast comment for listing {ListingId}", listingId);
            }
        }
        return dto;
    }

    public async Task DeleteCommentAsync(string userId, Guid commentId, bool isAdmin)
    {
        var comment = await _db.BusinessListingComments
            .Include(c => c.Listing)
            .FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed.");

        _db.BusinessListingComments.Remove(comment);
        if (comment.Listing != null && comment.Listing.CommentCount > 0)
            comment.Listing.CommentCount -= 1;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId,
            isAdmin ? "Admin.Business.Comment.Deleted" : "Business.Comment.Deleted",
            success: true, targetId: commentId.ToString());
    }

    public async Task HideCommentAsync(string adminId, Guid commentId, string reason)
    {
        var comment = await _db.BusinessListingComments.FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new KeyNotFoundException("Comment not found.");
        comment.IsHidden = true;
        comment.HiddenReason = reason;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(adminId, "Admin.Business.Comment.Hidden",
            success: true, targetId: commentId.ToString(), details: reason);
    }

    public async Task<Guid> ReportAsync(string userId, Guid listingId, ReportBusinessListingRequest request, string? ipAddress)
    {
        // Delegate to the existing reports pipeline — TargetType=BusinessListing is resolved
        // to the owning user inside AbuseReportService.ResolveTargetAsync (Phase 4 extension).
        var submit = new SubmitReportRequest(
            ReportTargetType.BusinessListing,
            listingId.ToString(),
            request.Reason,
            request.Description);
        return await _reports.SubmitAsync(submit, userId, ipAddress);
    }
}
