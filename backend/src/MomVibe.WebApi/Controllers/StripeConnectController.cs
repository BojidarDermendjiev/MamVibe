namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Application.Interfaces;
using Application.DTOs.Payments;
using Domain.Exceptions;

/// <summary>
/// Stripe Connect Express onboarding for peer-to-peer item sellers. Returns the
/// short-lived hosted-onboarding URL, the current local status snapshot, and a
/// dashboard login link for already-verified sellers.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/connect")]
[Authorize]
public class StripeConnectController : ControllerBase
{
    private readonly IStripeConnectService _connect;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _configuration;

    public StripeConnectController(
        IStripeConnectService connect,
        ICurrentUserService currentUser,
        IConfiguration configuration)
    {
        _connect = connect;
        _currentUser = currentUser;
        _configuration = configuration;
    }

    /// <summary>Returns the current user's Connect status snapshot — cheap local read.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<StripeConnectStatusDto>> Status([FromQuery] bool refresh = false)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var snapshot = refresh
            ? await _connect.RefreshStatusFromStripeAsync(userId)
            : await _connect.GetStatusAsync(userId);
        return Ok(snapshot);
    }

    /// <summary>
    /// Idempotently creates a Stripe Express account (if missing) and returns a
    /// hosted-onboarding URL to redirect the user to. URL expires in ~5 min.
    /// </summary>
    [HttpPost("onboard")]
    public async Task<ActionResult<StripeConnectOnboardingLinkDto>> Onboard()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var baseUrl = _configuration["Frontend:BaseUrl"]
            ?? Request.Headers.Origin.FirstOrDefault()
            ?? $"{Request.Scheme}://{Request.Host}";
        var returnUrl = $"{baseUrl}/dashboard?connect=return";
        var refreshUrl = $"{baseUrl}/dashboard?connect=refresh";
        var link = await _connect.CreateOnboardingLinkAsync(userId, returnUrl, refreshUrl);
        return Ok(link);
    }

    /// <summary>
    /// One-time login link to the seller's Stripe Express dashboard. Only valid
    /// for verified accounts; throws 409 <c>connect_not_verified</c> otherwise.
    /// </summary>
    [HttpPost("dashboard-link")]
    public async Task<ActionResult<StripeConnectDashboardLinkDto>> DashboardLink()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var link = await _connect.CreateDashboardLinkAsync(userId);
        return Ok(link);
    }
}
