namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.Shipping;
using MomVibe.WebApi.Hubs;

/// <summary>
/// SignalR-backed implementation of <see cref="IShipmentNotifier"/>.
/// Pushes shipment events to per-user groups (<c>user_{userId}</c>)
/// already maintained by <see cref="ChatHub"/>.
/// </summary>
public class SignalRShipmentNotifier : IShipmentNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    /// <summary>Initializes a new instance of <see cref="SignalRShipmentNotifier"/> with the ChatHub context.</summary>
    public SignalRShipmentNotifier(IHubContext<ChatHub, IChatClient> hub)
    {
        this._hub = hub;
    }

    /// <inheritdoc/>
    public Task NotifySellerShipmentReadyAsync(string sellerId, ShipmentDto shipment)
        => this._hub.Clients.Group($"user_{sellerId}").ShipmentCreated(shipment);

    /// <inheritdoc/>
    public Task NotifyShipmentStatusChangedAsync(string buyerId, ShipmentDto shipment)
        => this._hub.Clients.Group($"user_{buyerId}").ShipmentStatusChanged(shipment);
}
