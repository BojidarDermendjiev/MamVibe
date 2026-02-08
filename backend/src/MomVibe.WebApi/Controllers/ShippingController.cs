namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Shipping;

/// <summary>
/// API controller for shipping operations:
/// - Calculate shipping price for a courier/delivery type combination.
/// - Create shipments (waybills) with Econt or Speedy.
/// - Download shipping labels, track shipments, cancel shipments.
/// - List courier offices/lockers.
/// - Retrieve shipments by payment or by current user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShippingController"/>.
    /// </summary>
    /// <param name="shippingService">Service handling shipping logic.</param>
    /// <param name="currentUserService">Service providing current user context.</param>
    public ShippingController(IShippingService shippingService, ICurrentUserService currentUserService)
    {
        this._shippingService = shippingService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Calculates the shipping price for the given parameters.
    /// </summary>
    /// <param name="request">Calculation parameters including courier, delivery type, weight, etc.</param>
    /// <returns>200 OK with calculated price result.</returns>
    [Authorize]
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculatePrice([FromBody] CalculateShippingDto request)
    {
        var result = await this._shippingService.CalculatePriceAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Creates a shipment (waybill) with the specified courier.
    /// </summary>
    /// <param name="request">Shipment creation parameters.</param>
    /// <returns>200 OK with the created shipment details.</returns>
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentDto request)
    {
        var result = await this._shippingService.CreateShipmentAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Downloads the shipping label PDF for the specified shipment.
    /// </summary>
    /// <param name="id">The shipment ID.</param>
    /// <returns>PDF file content or 404 if not found.</returns>
    [Authorize]
    [HttpGet("{id:guid}/label")]
    public async Task<IActionResult> GetLabel(Guid id)
    {
        var pdf = await this._shippingService.GetLabelAsync(id);
        return File(pdf, "application/pdf", "label.pdf");
    }

    /// <summary>
    /// Gets tracking events for the specified shipment.
    /// </summary>
    /// <param name="id">The shipment ID.</param>
    /// <returns>200 OK with list of tracking events.</returns>
    [Authorize]
    [HttpGet("{id:guid}/track")]
    public async Task<IActionResult> TrackShipment(Guid id)
    {
        var events = await this._shippingService.TrackShipmentAsync(id);
        return Ok(events);
    }

    /// <summary>
    /// Cancels the specified shipment.
    /// </summary>
    /// <param name="id">The shipment ID.</param>
    /// <returns>200 OK on success.</returns>
    [Authorize]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelShipment(Guid id)
    {
        await this._shippingService.CancelShipmentAsync(id);
        return Ok();
    }

    /// <summary>
    /// Lists courier offices/lockers for the specified provider and optional city filter.
    /// </summary>
    /// <param name="provider">Courier provider enum value (0=Econt, 1=Speedy).</param>
    /// <param name="city">Optional city name to filter offices.</param>
    /// <returns>200 OK with list of offices.</returns>
    [Authorize]
    [HttpGet("offices")]
    public async Task<IActionResult> GetOffices([FromQuery] CourierProvider provider, [FromQuery] string? city = null)
    {
        var offices = await this._shippingService.GetOfficesAsync(provider, city);
        return Ok(offices);
    }

    /// <summary>
    /// Gets the shipment associated with a specific payment.
    /// </summary>
    /// <param name="paymentId">The payment ID.</param>
    /// <returns>200 OK with shipment details or 404 if not found.</returns>
    [Authorize]
    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetShipmentByPayment(Guid paymentId)
    {
        var shipment = await this._shippingService.GetShipmentByPaymentIdAsync(paymentId);
        if (shipment == null) return NotFound();
        return Ok(shipment);
    }

    /// <summary>
    /// Gets all shipments for the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.
    /// 200 OK with the user's shipment list.
    /// </returns>
    [Authorize]
    [HttpGet("my-shipments")]
    public async Task<IActionResult> MyShipments()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var shipments = await this._shippingService.GetShipmentsByUserAsync(userId);
        return Ok(shipments);
    }
}
