namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.PurchaseRequests;
using MomVibe.WebApi.Hubs;

/// <summary>
/// SignalR-backed implementation of <see cref="IPurchaseRequestNotifier"/>.
/// Pushes purchase-request events to per-user groups (<c>user_{userId}</c>)
/// already maintained by <see cref="ChatHub"/>.
/// </summary>
public class SignalRPurchaseRequestNotifier : IPurchaseRequestNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    /// <summary>Initializes a new instance of <see cref="SignalRPurchaseRequestNotifier"/> with the ChatHub context.</summary>
    public SignalRPurchaseRequestNotifier(IHubContext<ChatHub, IChatClient> hub)
    {
        this._hub = hub;
    }

    /// <inheritdoc/>
    public Task NotifySellerAsync(string sellerId, PurchaseRequestDto request)
        => this._hub.Clients.Group($"user_{sellerId}").ReceivePurchaseRequest(request);

    /// <inheritdoc/>
    public Task NotifyBuyerAsync(string buyerId, PurchaseRequestDto request)
        => this._hub.Clients.Group($"user_{buyerId}").PurchaseRequestUpdated(request);

    /// <inheritdoc/>
    public Task NotifyPaymentChosenAsync(string sellerId, Guid requestId, string paymentMethod, string buyerName)
        => this._hub.Clients.Group($"user_{sellerId}").PaymentMethodChosen(new
        {
            requestId,
            paymentMethod,
            buyerName
        });
}
