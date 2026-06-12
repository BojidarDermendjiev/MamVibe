namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;
using Application.DTOs.Business;
using Domain.Enums;

/// <summary>
/// Public + admin endpoints for <c>CoachReferral</c>. Submission is rate-limited (3/hour/IP)
/// and Turnstile-gated; the admin queue + status transitions are gated by the AdminOnly policy.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/coach-referrals")]
public class CoachReferralsController : ControllerBase
{
    private readonly ICoachReferralService _referrals;
    private readonly ICurrentUserService _currentUser;

    public CoachReferralsController(ICoachReferralService referrals, ICurrentUserService currentUser)
    {
        _referrals = referrals;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Public submission of a coach referral. Anonymous-allowed; Turnstile gate fires in
    /// non-development environments. Rate limited at <c>CoachReferralSubmit</c> (3/hour/IP).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.CoachReferralSubmit)]
    public async Task<ActionResult<object>> Submit([FromBody] SubmitCoachReferralRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var referrerUserId = _currentUser.UserId;
        var id = await _referrals.SubmitAsync(request, referrerUserId, ip);
        return Ok(new { id });
    }
}

/// <summary>Admin slice of the coach-referrals API: paged queue + status transitions.</summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/coach-referrals")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminCoachReferralsController : ControllerBase
{
    private readonly ICoachReferralService _referrals;
    private readonly ICurrentUserService _currentUser;

    public AdminCoachReferralsController(ICoachReferralService referrals, ICurrentUserService currentUser)
    {
        _referrals = referrals;
        _currentUser = currentUser;
    }

    /// <summary>Paged admin queue — filter by status (Submitted / Contacted / Onboarded / Rejected).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedReferralsResult>> List(
        [FromQuery] CoachReferralStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _referrals.AdminListAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Transition the referral to a new status. Promoter activations counter is bumped when Onboarded.</summary>
    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCoachReferralStatusRequest request)
    {
        var adminId = _currentUser.UserId;
        if (adminId == null) return Unauthorized();
        await _referrals.UpdateStatusAsync(id, request, adminId);
        return NoContent();
    }
}
