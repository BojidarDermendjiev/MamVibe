namespace MomVibe.Application.Interfaces;

using DTOs.Offers;

public interface IOfferNotifier
{
    Task NotifySellerAsync(string sellerId, OfferDto offer);
    Task NotifyBuyerAsync(string buyerId, OfferDto offer);
}
