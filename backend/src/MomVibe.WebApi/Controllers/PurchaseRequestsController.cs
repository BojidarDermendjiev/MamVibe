namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;

/// <summary>
/// API endpoints for the buyer purchase/reservation request workflow.
/// </summary>
[ApiController]
[Route("api/purchase-requests")]
[Authorize]
public class PurchaseRequestsController : ControllerBase
{
    private readonly IPurchaseRequestService _service;
    private readonly ICurrentUserService _currentUser;

    public PurchaseRequestsController(IPurchaseRequestService service, ICurrentUserService currentUser)
    {
        this._service = service;
        this._currentUser = currentUser;
    }

    /// <summary>
    /// Creates a purchase request for the given item. Atomically locks the item
    /// so no other buyer can request it simultaneously.
    /// Returns 409 Conflict if the item is already reserved.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequestBody body)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await this._service.CreateAsync(body.ItemId, userId);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Seller accepts a pending request.
    /// For Donate items a Booking payment is created immediately.
    /// For Sell items the buyer is notified to complete payment.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await this._service.AcceptAsync(id, userId);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Seller declines a pending request. The item is returned to the shop.
    /// </summary>
    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await this._service.DeclineAsync(id, userId);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Buyer notifies the seller which payment method they chose for an accepted request.
    /// The actual payment is still created through the existing /payments endpoints.
    /// </summary>
    [HttpPost("{id:guid}/payment-chosen")]
    public async Task<IActionResult> PaymentChosen(Guid id, [FromBody] PaymentChosenBody body)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await this._service.NotifyPaymentChosenAsync(id, userId, body.PaymentMethod);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Returns all purchase requests where the current user is the seller.</summary>
    [HttpGet("as-seller")]
    public async Task<IActionResult> GetAsSeller()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        var requests = await this._service.GetAsSellerAsync(userId);
        return Ok(requests);
    }

    /// <summary>Returns all purchase requests where the current user is the buyer.</summary>
    [HttpGet("as-buyer")]
    public async Task<IActionResult> GetAsBuyer()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        var requests = await this._service.GetAsBuyerAsync(userId);
        return Ok(requests);
    }
}

public record CreatePurchaseRequestBody(Guid ItemId);
public record PaymentChosenBody(string PaymentMethod);
