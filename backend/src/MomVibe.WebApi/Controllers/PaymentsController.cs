namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Payments;

/// <summary>
/// API controller for payment operations:
/// - Create Stripe checkout sessions (authenticated)
/// - Create on-spot payments (authenticated)
/// - Handle Stripe webhooks
/// - Retrieve current user's payments (authenticated)
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/>.
    /// </summary>
    /// <param name="paymentService">Service handling payment logic.</param>
    /// <param name="currentUserService">Service providing current user context.</param>
    /// <param name="configuration">Application configuration for frontend URLs.</param>
    public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUserService, IConfiguration configuration)
    {
        this._paymentService = paymentService;
        this._currentUserService = currentUserService;
        this._configuration = configuration;
    }

    /// <summary>
    /// Creates a Stripe checkout session for the specified item.
    /// </summary>
    /// <param name="itemId">The GUID of the item to purchase.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with a session URL for redirection on success.
    /// </returns>
    [Authorize]
    [HttpPost("checkout/{itemId:guid}")]
    public async Task<IActionResult> CreateCheckout(Guid itemId)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
            var sessionUrl = await this._paymentService.CreateCheckoutSessionAsync(
                itemId, userId,
                $"{frontendUrl}/payment/success",
                $"{frontendUrl}/payment/cancel");
            return Ok(new { sessionUrl });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Payment service error. Please check Stripe configuration.", details = ex.Message });
        }
    }

    /// <summary>
    /// Creates an on-spot (immediate) payment record for the specified item.
    /// </summary>
    /// <param name="itemId">The GUID of the item to purchase.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the created payment details on success.
    /// </returns>
    [Authorize]
    [HttpPost("onspot/{itemId:guid}")]
    public async Task<IActionResult> CreateOnSpotPayment(Guid itemId)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var payment = await this._paymentService.CreateOnSpotPaymentAsync(itemId, userId);
        return Ok(payment);
    }

    /// <summary>
    /// Creates a free booking for a donated item.
    /// </summary>
    /// <param name="itemId">The GUID of the donate item to book.</param>
    [Authorize]
    [HttpPost("booking/{itemId:guid}")]
    public async Task<IActionResult> CreateBooking(Guid itemId)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payment = await this._paymentService.CreateBookingAsync(itemId, userId);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for inline card payment.
    /// </summary>
    /// <param name="itemId">The GUID of the item to purchase.</param>
    /// <returns>
    /// 200 OK with the client secret for Stripe Elements.
    /// </returns>
    [Authorize]
    [HttpPost("create-intent/{itemId:guid}")]
    public async Task<IActionResult> CreatePaymentIntent(Guid itemId)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var clientSecret = await this._paymentService.CreatePaymentIntentAsync(itemId, userId);
            return Ok(new { clientSecret });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Payment service error. Please check Stripe configuration.", details = ex.Message });
        }
    }

    /// <summary>
    /// Stripe webhook endpoint to process asynchronous payment events.
    /// </summary>
    /// <remarks>
    /// Expects the raw request body and the <c>Stripe-Signature</c> header to validate the event.
    /// </remarks>
    /// <returns>
    /// 200 OK after successfully handling the event.
    /// </returns>
    /// <summary>
    /// Creates a Stripe checkout session for multiple items (bulk cart checkout).
    /// </summary>
    [Authorize]
    [HttpPost("bulk-checkout")]
    public async Task<IActionResult> BulkCheckout([FromBody] BulkCheckoutRequest request)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
            var sessionUrl = await this._paymentService.CreateBulkCheckoutSessionAsync(
                request.ItemIds, userId,
                $"{frontendUrl}/payment/success",
                $"{frontendUrl}/payment/cancel");
            return Ok(new { sessionUrl });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Payment service error.", details = ex.Message });
        }
    }

    /// <summary>
    /// Creates booking records for multiple donated items.
    /// </summary>
    [Authorize]
    [HttpPost("bulk-booking")]
    public async Task<IActionResult> BulkBooking([FromBody] BulkCheckoutRequest request)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payments = await this._paymentService.CreateBulkBookingAsync(request.ItemIds, userId);
            return Ok(payments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates on-spot payment records for multiple items.
    /// </summary>
    [Authorize]
    [HttpPost("bulk-onspot")]
    public async Task<IActionResult> BulkOnSpot([FromBody] BulkCheckoutRequest request)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payments = await this._paymentService.CreateBulkOnSpotPaymentAsync(request.ItemIds, userId);
            return Ok(payments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(this.HttpContext.Request.Body).ReadToEndAsync();
        var signature = this.Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";
        await this._paymentService.HandleWebhookAsync(json, signature);
        return Ok();
    }

    /// <summary>
    /// Retrieves all payments made by the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the user's payment history on success.
    /// </returns>
    [Authorize]
    [HttpGet("my-payments")]
    public async Task<IActionResult> MyPayments()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var payments = await this._paymentService.GetPaymentsByUserAsync(userId);
        return Ok(payments);
    }
}
