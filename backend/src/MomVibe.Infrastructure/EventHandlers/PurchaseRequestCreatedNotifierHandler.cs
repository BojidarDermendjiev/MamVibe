namespace MomVibe.Infrastructure.EventHandlers;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.DTOs.PurchaseRequests;
using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="PurchaseRequestCreatedEvent"/>, pushes a real-time SignalR notification
/// to the seller so their dashboard reflects the new request without a refresh.
/// </summary>
public sealed class PurchaseRequestCreatedNotifierHandler : INotificationHandler<PurchaseRequestCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPurchaseRequestNotifier _notifier;
    private readonly ILogger<PurchaseRequestCreatedNotifierHandler> _logger;

    public PurchaseRequestCreatedNotifierHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IPurchaseRequestNotifier notifier,
        ILogger<PurchaseRequestCreatedNotifierHandler> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._notifier = notifier;
        this._logger = logger;
    }

    public async Task Handle(PurchaseRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var request = await this._context.PurchaseRequests
                .AsNoTracking()
                .Include(r => r.Item).ThenInclude(i => i!.Photos)
                .Include(r => r.Bundle).ThenInclude(b => b!.BundleItems).ThenInclude(bi => bi.Item).ThenInclude(i => i.Photos)
                .Include(r => r.Buyer)
                .FirstOrDefaultAsync(r => r.Id == notification.RequestId, cancellationToken);
            if (request is null) return;

            var dto = this._mapper.Map<PurchaseRequestDto>(request);
            await this._notifier.NotifySellerAsync(request.SellerId, dto);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Purchase-request seller notification failed for request {RequestId}", notification.RequestId);
        }
    }
}
