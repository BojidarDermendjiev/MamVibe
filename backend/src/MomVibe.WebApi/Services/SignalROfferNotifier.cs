namespace MomVibe.WebApi.Services;

using Microsoft.AspNetCore.SignalR;

using Application.Interfaces;
using Application.DTOs.Offers;
using MomVibe.WebApi.Hubs;

public class SignalROfferNotifier : IOfferNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public SignalROfferNotifier(IHubContext<ChatHub, IChatClient> hub)
    {
        this._hub = hub;
    }

    public Task NotifySellerAsync(string sellerId, OfferDto offer)
        => this._hub.Clients.Group($"user_{sellerId}").ReceiveOffer(offer);

    public Task NotifyBuyerAsync(string buyerId, OfferDto offer)
        => this._hub.Clients.Group($"user_{buyerId}").OfferUpdated(offer);
}
