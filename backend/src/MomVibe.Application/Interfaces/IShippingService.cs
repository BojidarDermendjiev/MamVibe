namespace MomVibe.Application.Interfaces;

using DTOs.Shipping;
using Domain.Enums;

public interface IShippingService
{
    Task<ShippingPriceResultDto> CalculatePriceAsync(CalculateShippingDto request);
    Task<ShipmentDto> CreateShipmentAsync(CreateShipmentDto request);
    Task<byte[]> GetLabelAsync(Guid shipmentId, string userId);
    Task<List<TrackingEventDto>> TrackShipmentAsync(Guid shipmentId, string userId, bool isAdmin = false);
    Task CancelShipmentAsync(Guid shipmentId, string userId);
    Task<List<CourierOfficeDto>> GetOfficesAsync(CourierProvider provider, string? city = null);
    Task<ShipmentDto?> GetShipmentByPaymentIdAsync(Guid paymentId, string userId);
    Task<List<ShipmentDto>> GetShipmentsByUserAsync(string userId);
    Task SyncShipmentStatusesAsync();
    Task<List<ShipmentDto>> GetAllShipmentsAsync(int page = 1, int pageSize = 50);
}
