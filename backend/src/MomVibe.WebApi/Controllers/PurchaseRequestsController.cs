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
    /// </summary>
    /// <param name="body">Request body containing the GUID of the item to request.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 409 Conflict if the item is already reserved by another buyer.<br/>
    /// 200 OK with the created purchase request details on success.
    /// </returns>
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
    /// <param name="id">The GUID of the purchase request to accept.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the authenticated user is not the item's seller.<br/>
    /// 404 Not Found if the purchase request does not exist.<br/>
    /// 400 Bad Request if the request is not in a pending state.<br/>
    /// 200 OK with the updated purchase request details on success.
    /// </returns>
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
    /// <param name="id">The GUID of the purchase request to decline.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the authenticated user is not the item's seller.<br/>
    /// 404 Not Found if the purchase request does not exist.<br/>
    /// 400 Bad Request if the request is not in a pending state.<br/>
    /// 200 OK with the updated purchase request details on success.
    /// </returns>
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
    /// <param name="id">The GUID of the purchase request to update.</param>
    /// <param name="body">Body containing the chosen payment method identifier (e.g., "card", "onspot", "revolut").</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the authenticated user is not the buyer of this request.<br/>
    /// 404 Not Found if the purchase request does not exist.<br/>
    /// 400 Bad Request if the request is not in an accepted state.<br/>
    /// 200 OK with the updated purchase request details on success.
    /// </returns>
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

    /// <summary>
    /// Returns all purchase requests where the current user is the seller.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the list of purchase requests on success.
    /// </returns>
    [HttpGet("as-seller")]
    public async Task<IActionResult> GetAsSeller()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        var requests = await this._service.GetAsSellerAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Returns all purchase requests where the current user is the buyer.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the list of purchase requests on success.
    /// </returns>
    [HttpGet("as-buyer")]
    public async Task<IActionResult> GetAsBuyer()
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        var requests = await this._service.GetAsBuyerAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Checks the buyer's reputation on nekorekten.com before the seller approves.
    /// Only the seller of the request may call this endpoint.
    /// </summary>
    /// <param name="id">The GUID of the purchase request whose buyer should be checked.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the authenticated user is not the item's seller.<br/>
    /// 404 Not Found if the purchase request does not exist.<br/>
    /// 200 OK with the reputation check result including any fraud reports found.
    /// </returns>
    [HttpGet("{id:guid}/buyer-check")]
    public async Task<IActionResult> CheckBuyer(Guid id)
    {
        var userId = this._currentUser.UserId;
        if (userId == null) return Unauthorized();

        try
        {
            var result = await this._service.CheckBuyerAsync(id, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

public record CreatePurchaseRequestBody(Guid ItemId);
public record PaymentChosenBody(string PaymentMethod);
