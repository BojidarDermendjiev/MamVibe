namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;

/// <summary>
/// Manages the calling user's <c>PromoterProfile</c>: idempotent self-registration,
/// reading the profile, and rendering the dashboard payload.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/promoter")]
[Authorize]
public class PromotersController : ControllerBase
{
    private readonly IPromoterService _promoter;
    private readonly ICurrentUserService _currentUser;

    public PromotersController(IPromoterService promoter, ICurrentUserService currentUser)
    {
        _promoter = promoter;
        _currentUser = currentUser;
    }

    /// <summary>Returns the calling user's promoter profile, or 404 when they haven't registered.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<PromoterProfileDto>> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var profile = await _promoter.GetMineAsync(userId);
        return profile == null ? NotFound() : Ok(profile);
    }

    /// <summary>
    /// Promotes the calling user — creates the <c>PromoterProfile</c> + generates a unique
    /// referral code + grants the Promoter role. Idempotent.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PromoterProfileDto>> Create()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var profile = await _promoter.CreateAsync(userId);
        return Ok(profile);
    }

    /// <summary>Returns the dashboard payload — profile + counters + 10 most recent referrals.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<PromoterDashboardDto>> Dashboard()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var result = await _promoter.GetDashboardAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
