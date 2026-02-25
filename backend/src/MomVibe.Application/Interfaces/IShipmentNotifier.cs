namespace MomVibe.Application.Interfaces;

using Application.DTOs.Shipping;

/// <summary>
/// Abstraction for pushing real-time shipment notifications.
/// Implemented in the WebApi layer (SignalR) to avoid Infrastructure → WebApi circular dependency.
/// </summary>
public interface IShipmentNotifier
{
    /// <summary>
    /// Notifies the SELLER that a waybill has been generated for their item.
    /// They should print it and attach it to the package before handing it to the courier.
    /// </summary>
    Task NotifySellerShipmentReadyAsync(string sellerId, ShipmentDto shipment);

    /// <summary>
    /// Notifies the BUYER that the courier has picked up their package and it is on its way.
    /// </summary>
    Task NotifyShipmentStatusChangedAsync(string buyerId, ShipmentDto shipment);
}
