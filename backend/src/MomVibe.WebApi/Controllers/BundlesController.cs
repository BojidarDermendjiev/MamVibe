namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.DTOs.Bundles;
using Application.DTOs.Payments;
using Application.Interfaces;

/// <summary>
/// Endpoints for seller-defined bundles — a group of items sold or donated together
/// at a combined price. Buyers can browse a bundle, request purchase, or pay via
/// Stripe Checkout, on-spot, or cash-on-delivery.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bundles")]
public class BundlesController : ControllerBase
{
    private readonly IBundleService _bundleService;
    private readonly IPurchaseRequestService _purchaseRequestService;
    private readonly ICurrentUserService _currentUserService;

    public BundlesController(
        IBundleService bundleService,
        IPurchaseRequestService purchaseRequestService,
        ICurrentUserService currentUserService)
    {
        this._bundleService = bundleService;
        this._purchaseRequestService = purchaseRequestService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a bundle by its GUID. Available to anonymous users.
    /// Sets <c>IsOwnedByCurrentUser</c> on the response when a valid session exists.
    /// </summary>
    /// <param name="id">The GUID of the bundle.</param>
    /// <returns>
    /// 200 OK with the bundle detail.<br/>
    /// 404 Not Found if no bundle with the given ID exists.
    /// </returns>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var bundle = await this._bundleService.GetByIdAsync(id);
            bundle.IsOwnedByCurrentUser = this._currentUserService.UserId != null
                && this._currentUserService.UserId == bundle.SellerId;
            return Ok(bundle);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    /// <summary>
    /// Returns all bundles created by the authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of the caller's bundles.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var bundles = await this._bundleService.GetMyAsync(userId);
        return Ok(bundles);
    }

    /// <summary>
    /// Creates a new bundle owned by the authenticated user.
    /// </summary>
    /// <param name="dto">Bundle creation data including a name and the list of item IDs to group.</param>
    /// <returns>
    /// 201 Created with the new bundle and a <c>Location</c> header pointing to <see cref="GetById"/>.<br/>
    /// 400 Bad Request if business rules are violated (e.g. items belong to different sellers).<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if any referenced item does not exist.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBundleDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var bundle = await this._bundleService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = bundle.Id }, bundle);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>
    /// Deletes a bundle. Only the owning seller may delete their bundle.
    /// </summary>
    /// <param name="id">The GUID of the bundle to delete.</param>
    /// <returns>
    /// 204 No Content on success.<br/>
    /// 400 Bad Request if the bundle cannot be deleted (e.g. active payment in progress).<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller does not own the bundle.<br/>
    /// 404 Not Found if no bundle with the given ID exists.
    /// </returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._bundleService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>
    /// Submits a purchase request for a bundle. The seller must accept before payment proceeds.
    /// </summary>
    /// <param name="id">The GUID of the bundle to request.</param>
    /// <returns>
    /// 200 OK with the created purchase request.<br/>
    /// 400 Bad Request if the bundle is unavailable or the buyer already has an open request.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the bundle does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("{id:guid}/request")]
    public async Task<IActionResult> RequestPurchase(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var request = await this._purchaseRequestService.CreateForBundleAsync(id, userId);
            return Ok(request);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>
    /// Creates a Stripe Checkout session for the authenticated buyer to pay for a bundle.
    /// </summary>
    /// <param name="id">The GUID of the bundle to purchase.</param>
    /// <param name="delivery">Optional courier and address details appended to the Stripe session metadata.</param>
    /// <returns>
    /// 200 OK with <c>{ url }</c> — the Stripe-hosted checkout URL to redirect the buyer to.<br/>
    /// 400 Bad Request if the bundle state prevents checkout.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the bundle does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("{id:guid}/payment/checkout")]
    public async Task<IActionResult> CreateCheckout(Guid id, [FromBody] PaymentDeliveryRequest? delivery = null)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = await this._bundleService.CreateCheckoutSessionAsync(
                id, userId,
                successUrl: $"{baseUrl}/payment/success",
                cancelUrl: $"{baseUrl}/payment/cancel",
                delivery);
            return Ok(new { url });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>
    /// Records an on-spot payment for a bundle (cash or in-person card, no Stripe session required).
    /// </summary>
    /// <param name="id">The GUID of the bundle being paid for.</param>
    /// <param name="delivery">Optional delivery details to associate with this payment.</param>
    /// <returns>
    /// 200 OK with the created payment record.<br/>
    /// 400 Bad Request if the bundle is not in a payable state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the bundle does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("{id:guid}/payment/on-spot")]
    public async Task<IActionResult> CreateOnSpot(Guid id, [FromBody] PaymentDeliveryRequest? delivery = null)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payment = await this._bundleService.CreateOnSpotPaymentAsync(id, userId, delivery);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>
    /// Records a cash-on-delivery payment for a bundle. Delivery details are required.
    /// </summary>
    /// <param name="id">The GUID of the bundle being paid for.</param>
    /// <param name="delivery">Courier and address details required for COD fulfilment.</param>
    /// <returns>
    /// 200 OK with the created payment record.<br/>
    /// 400 Bad Request if the bundle is not in a payable state.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 404 Not Found if the bundle does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("{id:guid}/payment/cod")]
    public async Task<IActionResult> CreateCod(Guid id, [FromBody] PaymentDeliveryRequest delivery)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payment = await this._bundleService.CreateCashOnDeliveryAsync(id, userId, delivery);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
