namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.DTOs.Bundles;
using Application.DTOs.Payments;
using Application.Interfaces;

[ApiController]
[Route("api/bundles")]
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

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var bundle = await this._bundleService.GetByIdAsync(id);
            return Ok(bundle);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var bundles = await this._bundleService.GetMyAsync(userId);
        return Ok(bundles);
    }

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
