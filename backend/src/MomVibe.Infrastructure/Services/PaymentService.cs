namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Domain.Enums;
using Application.Events;
using Application.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Payments;
using Application.DTOs.Shipping;

using PaymentEntity = Domain.Entities.Payment;

/// <summary>
/// Handles non-Stripe payment flows: on-spot, cash-on-delivery, booking (single and bulk),
/// and payment queries. Stripe-specific flows live in <see cref="StripePaymentService"/>.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IShippingService _shippingService;
    private readonly IPublisher _publisher;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IApplicationDbContext context,
        IMapper mapper,
        IShippingService shippingService,
        IPublisher publisher,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _shippingService = shippingService;
        _publisher = publisher;
        _logger = logger;
    }

    private async Task<PaymentEntity?> FindRecentByIdempotencyKeyAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var cutoff = DateTime.UtcNow.AddHours(-24);
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .Where(p => p.IdempotencyKey == key && p.CreatedAt > cutoff)
            .FirstOrDefaultAsync();
    }

    private async Task<List<PaymentEntity>> FindBulkByIdempotencyKeyAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return [];
        var prefix = key + "#";
        var cutoff = DateTime.UtcNow.AddHours(-24);
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .Where(p => p.IdempotencyKey != null
                     && p.IdempotencyKey.StartsWith(prefix)
                     && p.CreatedAt > cutoff)
            .ToListAsync();
    }

    private static string? BulkKeyFor(string? key, Guid itemId) =>
        string.IsNullOrWhiteSpace(key) ? null : $"{key}#{itemId}";

    private async Task CompletePurchaseRequestAsync(Guid itemId, string buyerId)
    {
        var request = await _context.PurchaseRequests
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.ItemId == itemId
                                   && r.BuyerId == buyerId
                                   && r.Status  == PurchaseRequestStatus.Accepted);
        if (request != null)
        {
            request.Status         = PurchaseRequestStatus.Completed;
            request.Item.IsActive   = false;
            request.Item.IsReserved = false;
            request.Item.IsSold     = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PaymentDto> CreateOnSpotPaymentAsync(
        Guid itemId, string buyerId,
        PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null)
    {
        var existing = await FindRecentByIdempotencyKeyAsync(idempotencyKey);
        if (existing != null) return _mapper.Map<PaymentDto>(existing);

        var item = await _context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");

        var payment = new PaymentEntity
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            Amount = item.Price ?? 0,
            PaymentMethod = Domain.Enums.PaymentMethod.OnSpot,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        if (delivery != null)
        {
            try
            {
                await _shippingService.CreateShipmentAsync(new CreateShipmentDto
                {
                    PaymentId = payment.Id,
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    RecipientName = delivery.RecipientName,
                    RecipientPhone = delivery.RecipientPhone,
                    City = delivery.City,
                    DeliveryAddress = delivery.Address,
                    OfficeId = delivery.OfficeId,
                    OfficeName = delivery.OfficeName,
                    Weight = delivery.Weight,
                    IsCod = false, CodAmount = 0, IsInsured = false, InsuredAmount = 0
                });
            }
            catch (Exception ex) { _logger.LogError(ex, "Auto-shipment creation failed for on-spot payment {PaymentId}.", payment.Id); }
        }

        try { await CompletePurchaseRequestAsync(itemId, buyerId); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", itemId, buyerId); }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateBookingAsync(
        Guid itemId, string buyerId,
        PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null)
    {
        var existing = await FindRecentByIdempotencyKeyAsync(idempotencyKey);
        if (existing != null) return _mapper.Map<PaymentDto>(existing);

        var item = await _context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Donate)
            throw new InvalidOperationException("Only donate items can be booked.");

        var payment = new PaymentEntity
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            Amount = 0,
            PaymentMethod = Domain.Enums.PaymentMethod.Booking,
            Status = PaymentStatus.Completed,
            IdempotencyKey = idempotencyKey
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        if (delivery != null)
        {
            try
            {
                await _shippingService.CreateShipmentAsync(new CreateShipmentDto
                {
                    PaymentId = payment.Id,
                    CourierProvider = delivery.CourierProvider,
                    DeliveryType = delivery.DeliveryType,
                    RecipientName = delivery.RecipientName,
                    RecipientPhone = delivery.RecipientPhone,
                    City = delivery.City,
                    DeliveryAddress = delivery.Address,
                    OfficeId = delivery.OfficeId,
                    OfficeName = delivery.OfficeName,
                    Weight = delivery.Weight,
                    IsCod = false, CodAmount = 0, IsInsured = false, InsuredAmount = 0
                });
            }
            catch (Exception ex) { _logger.LogError(ex, "Auto-shipment creation failed for booking payment {PaymentId}.", payment.Id); }
        }

        await _publisher.Publish(new PaymentCompletedEvent(payment.Id));

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateCashOnDeliveryAsync(
        Guid itemId, string buyerId,
        PaymentDeliveryRequest delivery, string? idempotencyKey = null)
    {
        var existing = await FindRecentByIdempotencyKeyAsync(idempotencyKey);
        if (existing != null) return _mapper.Map<PaymentDto>(existing);

        var item = await _context.Items.FindAsync(itemId);
        if (item == null) throw new KeyNotFoundException("Item not found.");
        if (item.ListingType != ListingType.Sell)
            throw new InvalidOperationException("Cash on delivery is only available for items listed for sale.");

        decimal shippingFee = 0;
        try
        {
            var calc = await _shippingService.CalculatePriceAsync(new CalculateShippingDto
            {
                CourierProvider = delivery.CourierProvider,
                DeliveryType = delivery.DeliveryType,
                ToCity = delivery.City,
                OfficeId = delivery.OfficeId,
                Weight = delivery.Weight,
                IsCod = true,
                CodAmount = item.Price ?? 0,
                IsInsured = false, InsuredAmount = 0
            });
            shippingFee = calc.Price;
        }
        catch (Exception ex) { _logger.LogError(ex, "Shipping price calculation failed for COD on item {ItemId}.", itemId); }

        var totalAmount = (item.Price ?? 0) + shippingFee;

        var payment = new PaymentEntity
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = item.UserId,
            Amount = totalAmount,
            PaymentMethod = Domain.Enums.PaymentMethod.CashOnDelivery,
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        try
        {
            await _shippingService.CreateShipmentAsync(new CreateShipmentDto
            {
                PaymentId = payment.Id,
                CourierProvider = delivery.CourierProvider,
                DeliveryType = delivery.DeliveryType,
                RecipientName = delivery.RecipientName,
                RecipientPhone = delivery.RecipientPhone,
                City = delivery.City,
                DeliveryAddress = delivery.Address,
                OfficeId = delivery.OfficeId,
                OfficeName = delivery.OfficeName,
                Weight = delivery.Weight,
                IsCod = true,
                CodAmount = totalAmount,
                IsInsured = false, InsuredAmount = 0
            });
        }
        catch (Exception ex) { _logger.LogError(ex, "Auto-shipment creation failed for COD payment {PaymentId}.", payment.Id); }

        try { await CompletePurchaseRequestAsync(itemId, buyerId); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to complete purchase request for item {ItemId}, buyer {BuyerId}.", itemId, buyerId); }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> CreateBulkBookingAsync(
        List<Guid> itemIds, string buyerId, string? idempotencyKey = null)
    {
        var existingBatch = await FindBulkByIdempotencyKeyAsync(idempotencyKey);
        if (existingBatch.Count > 0) return _mapper.Map<List<PaymentDto>>(existingBatch);

        var items = await _context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count) throw new KeyNotFoundException("One or more items not found.");

        var donateItems = items.Where(i => i.ListingType == ListingType.Donate).ToList();
        var payments = new List<PaymentEntity>();

        foreach (var item in donateItems)
        {
            var payment = new PaymentEntity
            {
                ItemId = item.Id,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = 0,
                PaymentMethod = Domain.Enums.PaymentMethod.Booking,
                Status = PaymentStatus.Completed,
                IdempotencyKey = BulkKeyFor(idempotencyKey, item.Id)
            };
            _context.Payments.Add(payment);
            payments.Add(payment);
        }

        await _context.SaveChangesAsync();

        foreach (var payment in payments)
            await _publisher.Publish(new PaymentCompletedEvent(payment.Id));

        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<List<PaymentDto>> CreateBulkOnSpotPaymentAsync(
        List<Guid> itemIds, string buyerId, string? idempotencyKey = null)
    {
        var existingBatch = await FindBulkByIdempotencyKeyAsync(idempotencyKey);
        if (existingBatch.Count > 0) return _mapper.Map<List<PaymentDto>>(existingBatch);

        var items = await _context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count) throw new KeyNotFoundException("One or more items not found.");

        var payments = new List<PaymentEntity>();

        foreach (var item in items)
        {
            var payment = new PaymentEntity
            {
                ItemId = item.Id,
                BuyerId = buyerId,
                SellerId = item.UserId,
                Amount = item.Price ?? 0,
                PaymentMethod = Domain.Enums.PaymentMethod.OnSpot,
                Status = PaymentStatus.Pending,
                IdempotencyKey = BulkKeyFor(idempotencyKey, item.Id)
            };
            _context.Payments.Add(payment);
            payments.Add(payment);
        }

        await _context.SaveChangesAsync();
        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsByUserAsync(string userId, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .Where(p => p.BuyerId == userId || p.SellerId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var payments   = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<PaymentDto>
        {
            Items      = _mapper.Map<List<PaymentDto>>(payments),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<PagedResult<PaymentDto>> GetAllPaymentsAsync(int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Item)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var payments   = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<PaymentDto>
        {
            Items      = _mapper.Map<List<PaymentDto>>(payments),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
