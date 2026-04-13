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
    public async Task<IActionResult> CreateCheckout(Guid itemId, [FromBody] PaymentDeliveryRequest? delivery = null)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
            var sessionUrl = await this._paymentService.CreateCheckoutSessionAsync(
                itemId, userId,
                $"{frontendUrl}/payment/success",
                $"{frontendUrl}/payment/cancel",
                delivery);
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
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
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
    public async Task<IActionResult> CreateOnSpotPayment(Guid itemId, [FromBody] PaymentDeliveryRequest? delivery = null)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var payment = await this._paymentService.CreateOnSpotPaymentAsync(itemId, userId, delivery);
        return Ok(payment);
    }

    /// <summary>
    /// Creates a free booking for a donated item.
    /// </summary>
    /// <param name="itemId">The GUID of the donate item to book.</param>
    [Authorize]
    [HttpPost("booking/{itemId:guid}")]
    public async Task<IActionResult> CreateBooking(Guid itemId, [FromBody] PaymentDeliveryRequest? delivery = null)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var payment = await this._paymentService.CreateBookingAsync(itemId, userId, delivery);
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
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
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

        if (request.ItemIds == null || request.ItemIds.Count == 0)
            return BadRequest(new { error = "No items provided." });
        if (request.ItemIds.Count > 50)
            return BadRequest(new { error = "Cannot process more than 50 items at once." });
        var itemIds = request.ItemIds.Distinct().ToList();

        try
        {
            var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
            var sessionUrl = await this._paymentService.CreateBulkCheckoutSessionAsync(
                itemIds, userId,
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
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
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

    /// <summary>
    /// Creates a Stripe checkout session for a donation.
    /// No authentication required — anyone can support MamVibe.
    /// Payouts go to whatever bank account (e.g. Revolut Business IBAN) is
    /// connected under Stripe Dashboard → Settings → Payouts.
    /// </summary>
    /// <summary>
    /// Creates a Stripe PaymentIntent for a mobile donation (returns clientSecret for PaymentSheet).
    /// No authentication required.
    /// </summary>
    [HttpPost("donation/intent")]
    public async Task<IActionResult> CreateDonationIntent([FromBody] DonationCheckoutRequest request)
    {
        try
        {
            var clientSecret = await this._paymentService.CreateDonationIntentAsync(request.Amount);
            return Ok(new { clientSecret });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
        }
    }

    [HttpPost("donation/checkout")]
    public async Task<IActionResult> CreateDonationCheckout([FromBody] DonationCheckoutRequest request)
    {
        try
        {
            var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
            var sessionUrl = await this._paymentService.CreateDonationCheckoutAsync(
                request.Amount,
                $"{frontendUrl}/payment/success",
                $"{frontendUrl}/donate");
            return Ok(new { sessionUrl });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Payment service error. Please try again later." });
        }
    }

    [HttpPost("webhook")]
    [RequestSizeLimit(65_536)] // 64KB max for webhook payloads
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(this.HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = this.Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";
        try
        {
            await this._paymentService.HandleWebhookAsync(json, signature);
            return Ok();
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
