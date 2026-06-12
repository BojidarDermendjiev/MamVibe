namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Parents-facing interactions on a <c>BusinessListing</c>: likes, comments, and report
/// submission. Report submission delegates to <see cref="IAbuseReportService"/> so the
/// existing duplicate-pending + threshold-signal pipeline handles business listings too.
/// </summary>
public interface IBusinessListingInteractionsService
{
    /// <summary>Likes the listing for the calling user (idempotent — no-op if already liked).</summary>
    Task<ListingLikeStateDto> LikeAsync(string userId, Guid listingId);

    /// <summary>Removes the calling user's like (idempotent — no-op if not liked).</summary>
    Task<ListingLikeStateDto> UnlikeAsync(string userId, Guid listingId);

    /// <summary>Lists comments — hidden comments are filtered out unless <paramref name="includeHidden"/> is true (admin).</summary>
    Task<PagedCommentsResult> ListCommentsAsync(Guid listingId, int page, int pageSize, bool includeHidden = false);

    /// <summary>Adds a new comment under the listing.</summary>
    Task<BusinessListingCommentDto> AddCommentAsync(string userId, Guid listingId, CreateBusinessListingCommentRequest request);

    /// <summary>Deletes a comment — owner or admin only.</summary>
    Task DeleteCommentAsync(string userId, Guid commentId, bool isAdmin);

    /// <summary>Admin marks a comment hidden with a moderation reason; the row stays for audit.</summary>
    Task HideCommentAsync(string adminId, Guid commentId, string reason);

    /// <summary>Files an <c>AbuseReport</c> against this listing via the existing reports pipeline.</summary>
    Task<Guid> ReportAsync(string userId, Guid listingId, ReportBusinessListingRequest request, string? ipAddress);

    /// <summary>
    /// Records a public view of the listing — appends a <c>BusinessListingViewEvent</c>,
    /// increments the cached <c>ViewCount</c>, and broadcasts a <c>ListingViewed</c> delta
    /// to the owner's dashboard. <paramref name="viewerHash"/> dedupes refreshes per session.
    /// </summary>
    Task TrackViewAsync(Guid listingId, string viewerHash);
}
