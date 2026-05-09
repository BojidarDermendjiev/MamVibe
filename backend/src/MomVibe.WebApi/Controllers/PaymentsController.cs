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
    /// <param name="delivery">Optional delivery details (courier, address) to associate with the purchase.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 400 Bad Request if the purchase cannot proceed (e.g., item already sold).<br/>
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
    /// <param name="delivery">Optional delivery details to associate with the purchase.</param>
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
    /// <param name="delivery">Optional delivery details to associate with the booking.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 400 Bad Request if the booking cannot proceed (e.g., item already booked).<br/>
    /// 200 OK with the created booking details on success.
    /// </returns>
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
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the item does not exist.<br/>
    /// 400 Bad Request if the purchase cannot proceed.<br/>
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
    /// Creates a Stripe checkout session for multiple items (bulk cart checkout).
    /// </summary>
    /// <param name="request">Request body containing the list of item GUIDs to include in the session.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 400 Bad Request if no items are provided or the list exceeds 50 items.<br/>
    /// 404 Not Found if any item does not exist.<br/>
    /// 200 OK with a session URL for redirection on success.
    /// </returns>
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
    /// <param name="request">Request body containing the list of donated item GUIDs to book.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if any item does not exist.<br/>
    /// 400 Bad Request if any booking cannot proceed.<br/>
    /// 200 OK with the list of created booking records on success.
    /// </returns>
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
    /// <param name="request">Request body containing the list of item GUIDs to mark as on-spot purchases.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if any item does not exist.<br/>
    /// 400 Bad Request if any purchase cannot proceed.<br/>
    /// 200 OK with the list of created payment records on success.
    /// </returns>
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
    /// Creates a Stripe PaymentIntent for a mobile donation, returning a clientSecret for use with Stripe PaymentSheet.
    /// No authentication required — anyone can support MamVibe.
    /// </summary>
    /// <param name="request">Request body containing the donation amount in the smallest currency unit (e.g., stotinki).</param>
    /// <returns>
    /// 400 Bad Request if the amount is invalid.<br/>
    /// 200 OK with the client secret for Stripe PaymentSheet on success.
    /// </returns>
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

    /// <summary>
    /// Creates a Stripe hosted checkout session for a donation.
    /// No authentication required — anyone can support MamVibe.
    /// </summary>
    /// <param name="request">Request body containing the donation amount in the smallest currency unit (e.g., stotinki).</param>
    /// <returns>
    /// 400 Bad Request if the amount is invalid.<br/>
    /// 200 OK with a session URL for redirection to the Stripe-hosted checkout page.
    /// </returns>
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

    /// <summary>
    /// Stripe webhook endpoint that processes asynchronous payment lifecycle events.
    /// Validates the request using the <c>Stripe-Signature</c> header before processing.
    /// </summary>
    /// <returns>
    /// 400 Bad Request if signature verification fails or the event cannot be processed.<br/>
    /// 200 OK after successfully handling the event.
    /// </returns>
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
        catch (Stripe.StripeException)
        {
            // Do not expose Stripe exception details (signature mismatch, config errors) to the caller.
            // Stripe interprets any 4xx as "do not retry"; a generic message is sufficient.
            return BadRequest(new { error = "Webhook signature verification failed or event processing error." });
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
