namespace MomVibe.Application.Interfaces;

using DTOs.Shipping;
using Domain.Enums;

/// <summary>
/// Courier-agnostic abstraction for interacting with a delivery courier's API.
/// Each courier implementation (Econt, Speedy, etc.) provides its own mapping
/// to the courier's REST endpoints. To add a new courier, implement this interface
/// and register it in the DI container — no changes to existing code required.
/// </summary>
public interface ICourierProvider
{
    /// <summary>Gets the courier provider type this implementation represents.</summary>
    CourierProvider ProviderType { get; }

    /// <summary>Calculates the shipping price for the given delivery parameters.</summary>
    /// <param name="request">The delivery details required to compute the price.</param>
    Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request);

    /// <summary>Creates a shipment with the courier and returns the tracking number, waybill ID, and optional label URL.</summary>
    /// <param name="request">The shipment details including sender, recipient, and parcel information.</param>
    Task<(string TrackingNumber, string WaybillId, string? LabelUrl)> CreateShipmentAsync(CreateShipmentDto request);

    /// <summary>Downloads the shipping label PDF for the specified waybill.</summary>
    /// <param name="waybillId">The courier waybill identifier.</param>
    /// <returns>The raw PDF bytes of the shipping label.</returns>
    Task<byte[]> GetLabelAsync(string waybillId);

    /// <summary>Returns the tracking event history for the specified tracking number.</summary>
    /// <param name="trackingNumber">The courier-issued tracking number.</param>
    Task<List<TrackingEventDto>> TrackAsync(string trackingNumber);

    /// <summary>Cancels the shipment identified by the specified waybill ID.</summary>
    /// <param name="waybillId">The courier waybill identifier of the shipment to cancel.</param>
    Task CancelShipmentAsync(string waybillId);

    /// <summary>Returns a list of courier offices, optionally filtered by city.</summary>
    /// <param name="city">Optional city name filter.</param>
    Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null);
}
