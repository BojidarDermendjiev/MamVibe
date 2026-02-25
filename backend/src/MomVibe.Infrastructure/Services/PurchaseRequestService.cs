namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.PurchaseRequests;

/// <summary>
/// Handles the full lifecycle of purchase/reservation requests:
/// creation with atomic item-locking, seller accept/decline, and buyer payment-method notification.
/// Uses a database transaction in CreateAsync to guarantee that exactly one buyer can lock
/// an item at a time (race-condition safe).
/// </summary>
public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPurchaseRequestNotifier _notifier;

    public PurchaseRequestService(
        IApplicationDbContext context,
        IMapper mapper,
        IPurchaseRequestNotifier notifier)
    {
        this._context = context;
        this._mapper = mapper;
        this._notifier = notifier;
    }

    public async Task<PurchaseRequestDto> CreateAsync(Guid itemId, string buyerId)
    {
        await using var tx = await this._context.Database.BeginTransactionAsync();

        // Atomic check-and-lock: update IsActive → false only when the item is still active
        // AND has no existing Pending request. Executes as a single SQL UPDATE … WHERE …
        // so concurrent requests race at the database level; the second one updates 0 rows.
        var rowsAffected = await this._context.Items
            .Where(i => i.Id == itemId
                     && i.IsActive
                     && !this._context.PurchaseRequests
                            .Any(r => r.ItemId == itemId && r.Status == PurchaseRequestStatus.Pending))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsActive, false));

        if (rowsAffected == 0)
        {
            await tx.RollbackAsync();

            // Determine the exact failure reason for a meaningful error message
            var item = await this._context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
                throw new KeyNotFoundException("Item not found.");

            throw new InvalidOperationException("This item is not available for purchase requests.");
        }

        // Reload item (outside change-tracker, post-update) to get the seller id
        var lockedItem = await this._context.Items.AsNoTracking().FirstAsync(i => i.Id == itemId);

        if (lockedItem.UserId == buyerId)
        {
            // Revert the lock — owner cannot request their own item
            await this._context.Items
                .Where(i => i.Id == itemId)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsActive, true));
            await tx.RollbackAsync();
            throw new InvalidOperationException("You cannot request your own item.");
        }

        var request = new PurchaseRequest
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = lockedItem.UserId,
            Status = PurchaseRequestStatus.Pending
        };

        this._context.PurchaseRequests.Add(request);
        await this._context.SaveChangesAsync();
        await tx.CommitAsync();

        // Reload with navigations for the DTO
        var saved = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .FirstAsync(r => r.Id == request.Id);

        var dto = this._mapper.Map<PurchaseRequestDto>(saved);

        // Notify seller via SignalR (fire-and-forget; failure must not break the request)
        try { await this._notifier.NotifySellerAsync(saved.SellerId, dto); } catch { /* best effort */ }

        return dto;
    }

    public async Task<PurchaseRequestDto> AcceptAsync(Guid requestId, string sellerId)
    {
        var request = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("Purchase request not found.");
        if (request.SellerId != sellerId)
            throw new UnauthorizedAccessException("You are not the seller for this request.");
        if (request.Status != PurchaseRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be accepted.");

        request.Status = PurchaseRequestStatus.Accepted;

        // For donated items: create a free Booking payment immediately
        if (request.Item.ListingType == ListingType.Donate)
        {
            var payment = new Payment
            {
                ItemId = request.ItemId,
                BuyerId = request.BuyerId,
                SellerId = request.SellerId,
                Amount = 0,
                PaymentMethod = PaymentMethod.Booking,
                Status = PaymentStatus.Completed
            };
            this._context.Payments.Add(payment);
        }
        // For Sell items: item stays reserved (IsActive = false); buyer will choose payment method

        await this._context.SaveChangesAsync();

        var dto = this._mapper.Map<PurchaseRequestDto>(request);

        try { await this._notifier.NotifyBuyerAsync(request.BuyerId, dto); } catch { /* best effort */ }

        return dto;
    }

    public async Task<PurchaseRequestDto> DeclineAsync(Guid requestId, string sellerId)
    {
        var request = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("Purchase request not found.");
        if (request.SellerId != sellerId)
            throw new UnauthorizedAccessException("You are not the seller for this request.");
        if (request.Status != PurchaseRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be declined.");

        request.Status = PurchaseRequestStatus.Declined;
        request.Item.IsActive = true; // Return item to the shop

        await this._context.SaveChangesAsync();

        var dto = this._mapper.Map<PurchaseRequestDto>(request);

        try { await this._notifier.NotifyBuyerAsync(request.BuyerId, dto); } catch { /* best effort */ }

        return dto;
    }

    public async Task<PurchaseRequestDto> NotifyPaymentChosenAsync(Guid requestId, string buyerId, string paymentMethod)
    {
        var request = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("Purchase request not found.");
        if (request.BuyerId != buyerId)
            throw new UnauthorizedAccessException("You are not the buyer for this request.");
        if (request.Status != PurchaseRequestStatus.Accepted)
            throw new InvalidOperationException("Payment can only be confirmed for accepted requests.");

        var buyerName = request.Buyer?.DisplayName ?? buyerId;
        try
        {
            await this._notifier.NotifyPaymentChosenAsync(request.SellerId, requestId, paymentMethod, buyerName);
        }
        catch { /* best effort */ }

        return this._mapper.Map<PurchaseRequestDto>(request);
    }

    public async Task<List<PurchaseRequestDto>> GetAsSellerAsync(string sellerId)
    {
        var requests = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .Where(r => r.SellerId == sellerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var dtos = this._mapper.Map<List<PurchaseRequestDto>>(requests);

        // Enrich completed requests with ShipmentId (only when a shipment was actually created)
        // so the seller's "View Waybill" button only appears when there is a real waybill to view.
        var completedItemIds = requests
            .Where(r => r.Status == PurchaseRequestStatus.Completed)
            .Select(r => r.ItemId)
            .ToList();

        if (completedItemIds.Count > 0)
        {
            var paymentToShipment = await this._context.Payments
                .Where(p => completedItemIds.Contains(p.ItemId) && p.SellerId == sellerId)
                .Join(this._context.Shipments,
                    p => p.Id,
                    s => s.PaymentId,
                    (p, s) => new { p.ItemId, p.BuyerId, ShipmentId = s.Id })
                .ToListAsync();

            foreach (var dto in dtos)
            {
                var match = paymentToShipment.FirstOrDefault(x => x.ItemId == dto.ItemId && x.BuyerId == dto.BuyerId);
                if (match != null)
                    dto.ShipmentId = match.ShipmentId;
            }
        }

        return dtos;
    }

    public async Task<List<PurchaseRequestDto>> GetAsBuyerAsync(string buyerId)
    {
        var requests = await this._context.PurchaseRequests
            .Include(r => r.Item).ThenInclude(i => i.Photos)
            .Include(r => r.Buyer)
            .Where(r => r.BuyerId == buyerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return this._mapper.Map<List<PurchaseRequestDto>>(requests);
    }
}
