namespace MomVibe.Application.Interfaces;

using DTOs.Offers;

public interface IOfferService
{
    Task<OfferDto> CreateAsync(CreateOfferDto dto, string buyerId);
    Task<OfferDto> AcceptAsync(Guid offerId, string sellerId);
    Task<OfferDto> DeclineAsync(Guid offerId, string sellerId);
    Task<OfferDto> CounterAsync(Guid offerId, string sellerId, decimal counterPrice);
    Task<OfferDto> AcceptCounterAsync(Guid offerId, string buyerId);
    Task<OfferDto> DeclineCounterAsync(Guid offerId, string buyerId);
    Task<OfferDto> CancelAsync(Guid offerId, string buyerId);
    Task<List<OfferDto>> GetReceivedAsync(string sellerId);
    Task<List<OfferDto>> GetSentAsync(string buyerId);
}
