namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;

/// <summary>
/// Manages the single business profile owned by the calling user. All endpoints require
/// authentication; the Business role is granted automatically on first successful
/// <see cref="Create"/> (so the registration flow does not need to pre-promote the user).
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business/profile")]
[Authorize]
public class BusinessProfilesController : ControllerBase
{
    private readonly IBusinessProfileService _profile;
    private readonly ICurrentUserService _currentUser;

    public BusinessProfilesController(IBusinessProfileService profile, ICurrentUserService currentUser)
    {
        _profile = profile;
        _currentUser = currentUser;
    }

    /// <summary>Returns the calling user's profile or 404 when not created yet.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<BusinessProfileDto>> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var profile = await _profile.GetMineAsync(userId);
        return profile == null ? NotFound() : Ok(profile);
    }

    /// <summary>
    /// Creates the calling user's business profile.
    /// Returns 409 if a profile already exists; 403 with <c>device_already_has_business</c>
    /// when the fingerprint duplicate check fires; 404 when the supplied policy version is unknown.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BusinessProfileDto>> Create([FromBody] CreateBusinessProfileRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var profile = await _profile.CreateAsync(userId, request, ip, userAgent);
        return CreatedAtAction(nameof(GetMine), null, profile);
    }

    /// <summary>Updates the editable fields on the calling user's profile.</summary>
    [HttpPut]
    public async Task<ActionResult<BusinessProfileDto>> Update([FromBody] UpdateBusinessProfileRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var profile = await _profile.UpdateAsync(userId, request);
        return Ok(profile);
    }
}
