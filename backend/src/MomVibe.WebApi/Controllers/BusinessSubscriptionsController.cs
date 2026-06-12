namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;
using Application.DTOs.Business;

/// <summary>
/// Subscription lifecycle endpoints for the calling business owner:
/// plan listing, checkout session creation, Customer Portal link, cancellation, and
/// the dashboard read of the current subscription state.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business/subscription")]
public class BusinessSubscriptionsController : ControllerBase
{
    private readonly IBusinessSubscriptionService _subscriptions;
    private readonly ICurrentUserService _currentUser;

    public BusinessSubscriptionsController(
        IBusinessSubscriptionService subscriptions,
        ICurrentUserService currentUser)
    {
        _subscriptions = subscriptions;
        _currentUser = currentUser;
    }

    /// <summary>Returns the available plan tiers (Trial, Basic, Featured, Premium).</summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans()
    {
        var plans = await _subscriptions.GetPlansAsync();
        return Ok(plans);
    }

    /// <summary>Returns the calling user's subscription, or 404 when not yet subscribed.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<BusinessSubscriptionDto>> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var sub = await _subscriptions.GetMineAsync(userId);
        return sub == null ? NotFound() : Ok(sub);
    }

    /// <summary>Creates a Stripe Checkout session in subscription mode; returns the redirect URL.</summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<object>> CreateCheckout([FromBody] CreateSubscriptionCheckoutRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var url = await _subscriptions.CreateCheckoutUrlAsync(userId, request);
        return Ok(new { url });
    }

    /// <summary>Returns a Stripe Customer Portal URL for self-serve management.</summary>
    [HttpPost("portal")]
    [Authorize]
    public async Task<ActionResult<object>> CreateBillingPortal([FromBody] CreateBillingPortalRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var url = await _subscriptions.CreateBillingPortalUrlAsync(userId, request);
        return Ok(new { url });
    }

    /// <summary>Cancels the calling user's subscription. <c>atPeriodEnd=true</c> defers to period end.</summary>
    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromQuery] bool atPeriodEnd = true)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        await _subscriptions.CancelAsync(userId, atPeriodEnd);
        return NoContent();
    }
}
