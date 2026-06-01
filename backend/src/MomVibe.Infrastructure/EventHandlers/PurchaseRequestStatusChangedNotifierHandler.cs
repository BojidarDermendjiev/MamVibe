namespace MomVibe.Infrastructure.EventHandlers;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.DTOs.PurchaseRequests;
using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="PurchaseRequestStatusChangedEvent"/>, pushes a real-time SignalR notification
/// to the buyer so their request list reflects the seller's accept/decline decision instantly.
/// </summary>
public sealed class PurchaseRequestStatusChangedNotifierHandler : INotificationHandler<PurchaseRequestStatusChangedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPurchaseRequestNotifier _notifier;
    private readonly ILogger<PurchaseRequestStatusChangedNotifierHandler> _logger;

    public PurchaseRequestStatusChangedNotifierHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IPurchaseRequestNotifier notifier,
        ILogger<PurchaseRequestStatusChangedNotifierHandler> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._notifier = notifier;
        this._logger = logger;
    }

    public async Task Handle(PurchaseRequestStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var request = await this._context.PurchaseRequests
                .AsNoTracking()
                .Include(r => r.Item).ThenInclude(i => i!.Photos)
                .Include(r => r.Buyer)
                .FirstOrDefaultAsync(r => r.Id == notification.RequestId, cancellationToken);
            if (request is null) return;

            var dto = this._mapper.Map<PurchaseRequestDto>(request);
            await this._notifier.NotifyBuyerAsync(request.BuyerId, dto);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Purchase-request buyer notification failed for request {RequestId} (status={Status})", notification.RequestId, notification.NewStatus);
        }
    }
}
