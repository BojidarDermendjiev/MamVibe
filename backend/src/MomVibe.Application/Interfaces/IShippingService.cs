namespace MomVibe.Application.Interfaces;

using DTOs.Shipping;
using Domain.Enums;

/// <summary>
/// Defines operations for managing shipments through pluggable courier providers
/// (e.g., Econt, Speedy, BoxNow), including price calculation, label generation,
/// tracking, and status synchronisation.
/// </summary>
public interface IShippingService
{
    /// <summary>
    /// Calculates the shipping price for the given delivery parameters.
    /// </summary>
    /// <param name="request">The delivery details required to compute the price.</param>
    /// <returns>A <see cref="ShippingPriceResultDto"/> containing the calculated price breakdown.</returns>
    Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request);

    /// <summary>
    /// Creates a new shipment with the courier provider and persists the record.
    /// </summary>
    /// <param name="request">The shipment details, including sender, recipient, and parcel information.</param>
    /// <returns>The created <see cref="ShipmentDto"/>.</returns>
    Task<ShipmentDto> CreateShipmentAsync(CreateShipmentDto request);

    /// <summary>
    /// Retrieves the courier label PDF for the specified shipment.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="userId">The identifier of the requesting user; used to enforce ownership.</param>
    /// <returns>The raw PDF bytes of the shipping label.</returns>
    Task<byte[]> GetLabelAsync(Guid shipmentId, string userId);

    /// <summary>
    /// Retrieves the tracking event history for the specified shipment.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="userId">The identifier of the requesting user; used to enforce ownership.</param>
    /// <param name="isAdmin">When <c>true</c>, bypasses ownership checks.</param>
    /// <returns>An ordered list of <see cref="TrackingEventDto"/> instances.</returns>
    Task<List<TrackingEventDto>> TrackShipmentAsync(Guid shipmentId, string userId, bool isAdmin = false);

    /// <summary>
    /// Requests cancellation of the specified shipment from the courier provider.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment to cancel.</param>
    /// <param name="userId">The identifier of the requesting user; used to enforce ownership.</param>
    Task CancelShipmentAsync(Guid shipmentId, string userId);

    /// <summary>
    /// Returns the list of courier offices for the specified provider, optionally filtered by city.
    /// </summary>
    /// <param name="provider">The courier provider whose offices to retrieve.</param>
    /// <param name="city">Optional city name filter.</param>
    /// <returns>A list of <see cref="CourierOfficeDto"/> instances.</returns>
    Task<List<CourierOfficeDto>> GetOfficesAsync(CourierProvider provider, string? city = null);

    /// <summary>
    /// Retrieves the shipment associated with a specific payment, enforcing that the requesting
    /// user is a party to that payment.
    /// </summary>
    /// <param name="paymentId">The unique identifier of the payment.</param>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <returns>The matching <see cref="ShipmentDto"/>, or <c>null</c> if none exists.</returns>
    Task<ShipmentDto?> GetShipmentByPaymentIdAsync(Guid paymentId, string userId);

    /// <summary>
    /// Retrieves all shipments for which the specified user is the sender or recipient.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of <see cref="ShipmentDto"/> instances.</returns>
    Task<List<ShipmentDto>> GetShipmentsByUserAsync(string userId);

    /// <summary>
    /// Synchronises the statuses of all active shipments by polling each courier provider.
    /// Intended to be called from a background service on a scheduled interval.
    /// </summary>
    Task SyncShipmentStatusesAsync();

    /// <summary>
    /// Retrieves a paginated list of all shipments across all users (administrator use).
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <returns>A list of <see cref="ShipmentDto"/> instances.</returns>
    Task<List<ShipmentDto>> GetAllShipmentsAsync(int page = 1, int pageSize = 50);
}
