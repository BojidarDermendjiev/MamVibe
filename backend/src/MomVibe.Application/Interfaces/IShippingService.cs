namespace MomVibe.Application.Interfaces;

using DTOs.Shipping;
using Domain.Enums;

/// <summary>
/// Application-level shipping orchestrator:
/// - Delegates courier API calls to the correct provider via factory.
/// - Persists shipment entities after courier operations.
/// - Provides shipment queries by payment, user, and ID.
/// - Supports background status synchronization.
/// </summary>
public interface IShippingService
{
    Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request);
    Task<ShipmentDto> CreateShipmentAsync(CreateShipmentDto request);
    Task<byte[]> GetLabelAsync(Guid shipmentId);
    Task<List<TrackingEventDto>> TrackShipmentAsync(Guid shipmentId);
    Task CancelShipmentAsync(Guid shipmentId);
    Task<List<CourierOfficeDto>> GetOfficesAsync(CourierProvider provider, string? city = null);
    Task<ShipmentDto?> GetShipmentByPaymentIdAsync(Guid paymentId);
    Task<List<ShipmentDto>> GetShipmentsByUserAsync(string userId);
    Task SyncShipmentStatusesAsync();
    Task<List<ShipmentDto>> GetAllShipmentsAsync();
}
