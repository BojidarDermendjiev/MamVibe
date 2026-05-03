namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.UserRatings;

[ApiController]
[Route("api/purchase-requests/{purchaseRequestId:guid}/rating")]
public class UserRatingsController : ControllerBase
{
    private readonly IUserRatingService _ratingService;
    private readonly ICurrentUserService _currentUserService;

    public UserRatingsController(IUserRatingService ratingService, ICurrentUserService currentUserService)
    {
        this._ratingService = ratingService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Submits a star rating for the seller after a completed purchase.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Guid purchaseRequestId, [FromBody] CreateUserRatingDto dto)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(new { error = "Rating must be between 1 and 5." });

        try
        {
            var result = await _ratingService.CreateAsync(userId, purchaseRequestId, dto);
            return CreatedAtAction(nameof(Create), new { purchaseRequestId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

[ApiController]
[Route("api/users/{userId}/ratings")]
public class UserRatingsByUserController : ControllerBase
{
    private readonly IUserRatingService _ratingService;

    public UserRatingsByUserController(IUserRatingService ratingService)
    {
        this._ratingService = ratingService;
    }

    /// <summary>
    /// Returns all ratings received by a user (their seller reputation).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetForUser(string userId)
    {
        var ratings = await _ratingService.GetForUserAsync(userId);
        return Ok(ratings);
    }

    /// <summary>
    /// Returns the average rating and count for a user.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(string userId)
    {
        var (average, count) = await _ratingService.GetSummaryAsync(userId);
        return Ok(new { average, count });
    }
}
