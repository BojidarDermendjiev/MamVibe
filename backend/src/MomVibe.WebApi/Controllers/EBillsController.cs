namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;

/// <summary>
/// Exposes the buyer's electronic payment receipts (e-bills).
/// All endpoints require an authenticated user — a buyer can only see their own e-bills.
///
/// E-bills are generated automatically for completed Sell-item purchases (Card or Wallet).
/// Donations (Booking) and on-spot payments are excluded.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EBillsController : ControllerBase
{
    private readonly IEBillService _eBillService;
    private readonly ICurrentUserService _currentUserService;

    public EBillsController(IEBillService eBillService, ICurrentUserService currentUserService)
    {
        _eBillService = eBillService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns all e-bills for the authenticated buyer, newest first.
    /// Only completed purchases of Sell-type items are included.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyEBills()
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var bills = await _eBillService.GetMyEBillsAsync(userId);
        return Ok(bills);
    }

    /// <summary>
    /// Returns a single e-bill by payment ID.
    /// Returns 404 if not found, 403 if the bill belongs to a different buyer.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEBill(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var bill = await _eBillService.GetEBillAsync(id, userId);
            return Ok(bill);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Re-sends the receipt email for the specified e-bill to the buyer's registered address.
    /// Rate-limited to 3 requests per minute per user to prevent email abuse.
    /// </summary>
    [HttpPost("{id:guid}/resend")]
    [EnableRateLimiting(RateLimitPolicies.EBillResend)]
    public async Task<IActionResult> ResendEBill(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            await _eBillService.ResendEBillEmailAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
