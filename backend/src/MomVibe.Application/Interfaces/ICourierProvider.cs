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
    CourierProvider ProviderType { get; }
    Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request);
    Task<(string TrackingNumber, string WaybillId, string? LabelUrl)> CreateShipmentAsync(CreateShipmentDto request);
    Task<byte[]> GetLabelAsync(string waybillId);
    Task<List<TrackingEventDto>> TrackAsync(string trackingNumber);
    Task CancelShipmentAsync(string waybillId);
    Task<List<CourierOfficeDto>> GetOfficesAsync(string? city = null);
}
