namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;

[ApiController]
[Route("api/follows")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ICurrentUserService _currentUserService;

    public FollowsController(IFollowService followService, ICurrentUserService currentUserService)
    {
        this._followService = followService;
        this._currentUserService = currentUserService;
    }

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

    [Authorize]
    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowingAsync(userId);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowersAsync(userId);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._followService.GetFollowingFeedAsync(userId, page, pageSize);
        return Ok(result);
    }
}
