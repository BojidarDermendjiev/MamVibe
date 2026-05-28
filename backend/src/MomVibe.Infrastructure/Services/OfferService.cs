namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Offers;

public class OfferService : IOfferService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IOfferNotifier _notifier;

    public OfferService(IApplicationDbContext context, IMapper mapper, IOfferNotifier notifier)
    {
        this._context = context;
        this._mapper = mapper;
        this._notifier = notifier;
    }

    public async Task<OfferDto> CreateAsync(CreateOfferDto dto, string buyerId)
    {
        var item = await this._context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.IsActive);

        if (item == null)
            throw new KeyNotFoundException("Item not found or not available.");
        if (item.ListingType != Domain.Enums.ListingType.Sell)
            throw new InvalidOperationException("Offers can only be made on items for sale.");
        if (item.UserId == buyerId)
            throw new InvalidOperationException("You cannot make an offer on your own item.");

        // Prevent duplicate pending/countered offers from the same buyer
        var existing = await this._context.Offers
            .AnyAsync(o => o.ItemId == dto.ItemId && o.BuyerId == buyerId
                && (o.Status == OfferStatus.Pending || o.Status == OfferStatus.Countered));
        if (existing)
            throw new InvalidOperationException("You already have an active offer on this item.");

        var offer = new Offer
        {
            ItemId = dto.ItemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            OfferedPrice = dto.OfferedPrice,
            Status = OfferStatus.Pending,
        };

        this._context.Offers.Add(offer);
        await this._context.SaveChangesAsync();

        var saved = await this.LoadOfferAsync(offer.Id);
        var offerDto = this.MapOffer(saved);

        try { await this._notifier.NotifySellerAsync(saved.SellerId, offerDto); } catch { /* best effort */ }

        return offerDto;
    }

    public async Task<OfferDto> AcceptAsync(Guid offerId, string sellerId)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureSeller(offer, sellerId);
        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("Only pending offers can be accepted.");

        offer.Status = OfferStatus.Accepted;
        await this._context.SaveChangesAsync();

        var dto = this.MapOffer(offer);
        try { await this._notifier.NotifyBuyerAsync(offer.BuyerId, dto); } catch { /* best effort */ }
        return dto;
    }

    public async Task<OfferDto> DeclineAsync(Guid offerId, string sellerId)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureSeller(offer, sellerId);
        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("Only pending offers can be declined.");

        offer.Status = OfferStatus.Declined;
        await this._context.SaveChangesAsync();

        var dto = this.MapOffer(offer);
        try { await this._notifier.NotifyBuyerAsync(offer.BuyerId, dto); } catch { /* best effort */ }
        return dto;
    }

    public async Task<OfferDto> CounterAsync(Guid offerId, string sellerId, decimal counterPrice)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureSeller(offer, sellerId);
        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("Only pending offers can be countered.");

        offer.CounterPrice = counterPrice;
        offer.Status = OfferStatus.Countered;
        await this._context.SaveChangesAsync();

        var dto = this.MapOffer(offer);
        try { await this._notifier.NotifyBuyerAsync(offer.BuyerId, dto); } catch { /* best effort */ }
        return dto;
    }

    public async Task<OfferDto> AcceptCounterAsync(Guid offerId, string buyerId)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureBuyer(offer, buyerId);
        if (offer.Status != OfferStatus.Countered)
            throw new InvalidOperationException("Only countered offers can have their counter accepted.");

        offer.Status = OfferStatus.Accepted;
        await this._context.SaveChangesAsync();

        var dto = this.MapOffer(offer);
        try { await this._notifier.NotifySellerAsync(offer.SellerId, dto); } catch { /* best effort */ }
        return dto;
    }

    public async Task<OfferDto> DeclineCounterAsync(Guid offerId, string buyerId)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureBuyer(offer, buyerId);
        if (offer.Status != OfferStatus.Countered)
            throw new InvalidOperationException("Only countered offers can have their counter declined.");

        offer.Status = OfferStatus.Declined;
        await this._context.SaveChangesAsync();

        var dto = this.MapOffer(offer);
        try { await this._notifier.NotifySellerAsync(offer.SellerId, dto); } catch { /* best effort */ }
        return dto;
    }

    public async Task<OfferDto> CancelAsync(Guid offerId, string buyerId)
    {
        var offer = await this.LoadOfferAsync(offerId);
        this.EnsureBuyer(offer, buyerId);
        if (offer.Status != OfferStatus.Pending && offer.Status != OfferStatus.Countered)
            throw new InvalidOperationException("Only pending or countered offers can be cancelled.");

        offer.Status = OfferStatus.Cancelled;
        await this._context.SaveChangesAsync();

        return this.MapOffer(offer);
    }

    public async Task<List<OfferDto>> GetReceivedAsync(string sellerId)
    {
        var offers = await this._context.Offers
            .Include(o => o.Item).ThenInclude(i => i.Photos)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return offers.Select(this.MapOffer).ToList();
    }

    public async Task<List<OfferDto>> GetSentAsync(string buyerId)
    {
        var offers = await this._context.Offers
            .Include(o => o.Item).ThenInclude(i => i.Photos)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return offers.Select(this.MapOffer).ToList();
    }

    private async Task<Offer> LoadOfferAsync(Guid offerId)
    {
        var offer = await this._context.Offers
            .Include(o => o.Item).ThenInclude(i => i.Photos)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null)
            throw new KeyNotFoundException("Offer not found.");

        return offer;
    }

    private void EnsureSeller(Offer offer, string userId)
    {
        if (offer.SellerId != userId)
            throw new UnauthorizedAccessException("You are not the seller for this offer.");
    }

    private void EnsureBuyer(Offer offer, string userId)
    {
        if (offer.BuyerId != userId)
            throw new UnauthorizedAccessException("You are not the buyer for this offer.");
    }

    private OfferDto MapOffer(Offer o) => new OfferDto
    {
        Id = o.Id,
        ItemId = o.ItemId,
        ItemTitle = o.Item?.Title,
        ItemPhotoUrl = o.Item?.Photos?.OrderBy(p => p.DisplayOrder).Select(p => p.Url).FirstOrDefault(),
        ItemPrice = o.Item?.Price,
        BuyerId = o.BuyerId,
        BuyerDisplayName = o.Buyer?.DisplayName,
        BuyerAvatarUrl = o.Buyer?.AvatarUrl,
        SellerId = o.SellerId,
        SellerDisplayName = o.Seller?.DisplayName,
        OfferedPrice = o.OfferedPrice,
        CounterPrice = o.CounterPrice,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
    };
}
