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

    public SignalRPurchaseRequestNotifier(IHubContext<ChatHub, IChatClient> hub)
    {
        this._hub = hub;
    }

    public Task NotifySellerAsync(string sellerId, PurchaseRequestDto request)
        => this._hub.Clients.Group($"user_{sellerId}").ReceivePurchaseRequest(request);

    public Task NotifyBuyerAsync(string buyerId, PurchaseRequestDto request)
        => this._hub.Clients.Group($"user_{buyerId}").PurchaseRequestUpdated(request);

    public Task NotifyPaymentChosenAsync(string sellerId, Guid requestId, string paymentMethod, string buyerName)
        => this._hub.Clients.Group($"user_{sellerId}").PaymentMethodChosen(new
        {
            requestId,
            paymentMethod,
            buyerName
        });
}
