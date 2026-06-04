namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;

/// <summary>
/// Authenticated endpoints for the seller-follow social feature.
/// Users can follow sellers to receive new-listing notifications.
/// The toggle endpoint acts as a follow/unfollow switch depending on current state.
/// All authenticated endpoints are subject to the global rate-limit policy.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/follows")]
[EnableRateLimiting(RateLimitPolicies.Global)]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ICurrentUserService _currentUserService;

    public FollowsController(IFollowService followService, ICurrentUserService currentUserService)
    {
        this._followService = followService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Toggles the follow relationship between the authenticated user and the target seller.
    /// If the caller already follows <paramref name="userId"/>, the follow is removed; otherwise it is added.
    /// </summary>
    /// <param name="userId">The identity string of the seller to follow or unfollow.</param>
    /// <returns>
    /// 200 OK with <c>{ isFollowing, followerCount }</c> reflecting the new state.<br/>
    /// 400 Bad Request if the user tries to follow themselves.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the target user does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("{userId}")]
    public async Task<IActionResult> Toggle(string userId)
    {
        var currentUserId = this._currentUserService.UserId;
        if (currentUserId == null) return Unauthorized();
        try
        {
            var result = await this._followService.ToggleFollowAsync(currentUserId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>
    /// Returns the follow status between the authenticated user and the target seller,
    /// along with the seller's total follower count.
    /// </summary>
    /// <param name="userId">The identity string of the seller to query.</param>
    /// <returns>
    /// 200 OK with <c>{ isFollowing, followerCount }</c>.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [Authorize]
    [HttpGet("{userId}/status")]
    public async Task<IActionResult> GetStatus(string userId)
    {
        var currentUserId = this._currentUserService.UserId;
        if (currentUserId == null) return Unauthorized();
        var isFollowing = await this._followService.IsFollowingAsync(currentUserId, userId);
        var followerCount = await this._followService.GetFollowerCountAsync(userId);
        return Ok(new { isFollowing, followerCount });
    }

    /// <summary>
    /// Returns all sellers that the authenticated user currently follows.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of followed seller profiles.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [Authorize]
    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowingAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Returns all users who currently follow the authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of follower profiles.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [Authorize]
    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowersAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Returns a paginated feed of the latest items listed by sellers the authenticated user follows.
    /// Results are ordered by listing date descending (newest first).
    /// </summary>
    /// <param name="page">1-based page number. Values below 1 are clamped to 1.</param>
    /// <param name="pageSize">Items per page (1–50). Values outside this range default to 12.</param>
    /// <returns>
    /// 200 OK with a paginated list of items from followed sellers.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [Authorize]
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 12;
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowingFeedAsync(userId, page, pageSize);
        return Ok(result);
    }
}
