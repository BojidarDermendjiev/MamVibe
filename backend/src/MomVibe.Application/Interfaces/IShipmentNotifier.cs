namespace MomVibe.Application.Interfaces;

using Application.DTOs.Shipping;

/// <summary>
/// Abstraction for pushing real-time shipment notifications to buyers.
/// Implemented in the WebApi layer (SignalR) to avoid Infrastructure → WebApi circular dependency.
/// </summary>
public interface IShipmentNotifier
{
    /// <summary>Notifies the buyer that a new shipment waybill has been created for their order.</summary>
    Task NotifyShipmentCreatedAsync(string buyerId, ShipmentDto shipment);

    /// <summary>Notifies the buyer that the courier has picked up their package.</summary>
    Task NotifyShipmentStatusChangedAsync(string buyerId, ShipmentDto shipment);
}
