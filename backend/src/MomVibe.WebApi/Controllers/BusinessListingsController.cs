namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Enums;

/// <summary>
/// Public browse + detail and authenticated owner CRUD for <c>BusinessListing</c>.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business/listings")]
public class BusinessListingsController : ControllerBase
{
    private readonly IBusinessListingService _listings;
    private readonly IBusinessListingInteractionsService _interactions;
    private readonly ICurrentUserService _currentUser;

    public BusinessListingsController(
        IBusinessListingService listings,
        IBusinessListingInteractionsService interactions,
        ICurrentUserService currentUser)
    {
        _listings = listings;
        _interactions = interactions;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Paged public browse. Filters: <c>city</c> (substring), <c>activityType</c>, <c>ageMonths</c>
    /// (matches listings whose age range covers the value).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<BrowseListingsResult>> Browse(
        [FromQuery] BusinessCategory? category = null,
        [FromQuery] string? city = null,
        [FromQuery] ActivityType? activityType = null,
        [FromQuery] int? ageMonths = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _listings.BrowseAsync(category, city, activityType, ageMonths, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Returns the listing owned by the calling user (if any) so the dashboard
    /// can render edit / view-stats CTAs. Returns 404 when no listing exists.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<BusinessListingDto>> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var listing = await _listings.GetMineAsync(userId);
        return listing == null ? NotFound() : Ok(listing);
    }

    /// <summary>Public detail. Hidden / unapproved listings are 404 to anonymous callers.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessListingDto>> GetById(Guid id)
    {
        var includeHidden = _currentUser.IsAdmin;
        var listing = await _listings.GetByIdAsync(id, _currentUser.UserId, includeHidden);
        return listing == null ? NotFound() : Ok(listing);
    }

    /// <summary>Creates the calling user's single listing.</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BusinessListingDto>> Create([FromBody] CreateBusinessListingRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var listing = await _listings.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, listing);
    }

    /// <summary>Updates a listing — owner only (admin overrides via the admin endpoints).</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BusinessListingDto>> Update(Guid id, [FromBody] UpdateBusinessListingRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var listing = await _listings.UpdateAsync(userId, id, request, isAdmin: _currentUser.IsAdmin);
            return Ok(listing);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Deletes a listing — owner only (admins use the admin endpoints).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            await _listings.DeleteAsync(userId, id, isAdmin: _currentUser.IsAdmin);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ───── Interactions: likes ──────────────────────────────────────────────────

    /// <summary>Likes the listing for the calling user (idempotent).</summary>
    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<ActionResult<ListingLikeStateDto>> Like(Guid id)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var state = await _interactions.LikeAsync(userId, id);
        return Ok(state);
    }

    /// <summary>Removes the calling user's like (idempotent).</summary>
    [HttpDelete("{id:guid}/like")]
    [Authorize]
    public async Task<ActionResult<ListingLikeStateDto>> Unlike(Guid id)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var state = await _interactions.UnlikeAsync(userId, id);
        return Ok(state);
    }

    // ───── Interactions: comments ───────────────────────────────────────────────

    /// <summary>Paged comments on the listing. Hidden comments are visible to admin callers only.</summary>
    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedCommentsResult>> GetComments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _interactions.ListCommentsAsync(id, page, pageSize, includeHidden: _currentUser.IsAdmin);
        return Ok(result);
    }

    /// <summary>Posts a new comment under the listing.</summary>
    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<ActionResult<BusinessListingCommentDto>> PostComment(
        Guid id,
        [FromBody] CreateBusinessListingCommentRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var dto = await _interactions.AddCommentAsync(userId, id, request);
        return Ok(dto);
    }

    /// <summary>Deletes a comment — author or admin only.</summary>
    [HttpDelete("{id:guid}/comments/{commentId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await _interactions.DeleteCommentAsync(userId, commentId, isAdmin: _currentUser.IsAdmin);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ───── Interactions: report ─────────────────────────────────────────────────

    /// <summary>
    /// Submits an abuse report against this listing. Delegates to the existing reports pipeline
    /// (duplicate-pending guard, threshold signal emission). Self-reports are rejected.
    /// </summary>
    [HttpPost("{id:guid}/report")]
    [Authorize]
    public async Task<IActionResult> Report(Guid id, [FromBody] ReportBusinessListingRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var reportId = await _interactions.ReportAsync(userId, id, request, ip);
        return Ok(new { id = reportId });
    }

    // ───── Interactions: view tracking ──────────────────────────────────────────

    /// <summary>
    /// Records a public view of the listing for analytics. Anonymous-safe; viewer identity
    /// is hashed from the user id (when present) plus the truncated IP and user-agent so
    /// the same parent refreshing the page does not inflate counts.
    /// </summary>
    [HttpPost("{id:guid}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackView(Guid id)
    {
        var userId = _currentUser.UserId ?? "";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers.UserAgent.ToString();
        var viewerHash = ComputeViewerHash(userId, ip, ua);
        await _interactions.TrackViewAsync(id, viewerHash);
        return NoContent();
    }

    private static string ComputeViewerHash(string userId, string ip, string userAgent)
    {
        var raw = $"{userId}|{ip}|{userAgent}";
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}
