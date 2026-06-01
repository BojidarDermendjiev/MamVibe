namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Events;
using Application.Interfaces;
using Domain.Enums;

/// <summary>
/// On <see cref="PaymentCompletedEvent"/>, finds the matching accepted PurchaseRequest and
/// marks it Completed; also flips the underlying Item to <c>IsSold = true</c> and clears
/// active/reserved flags so it disappears from the browse listing.
///
/// Skipped for bundle payments — the bundle completion path lives in <c>PaymentService</c>
/// because it has to update every member item in the bundle atomically.
/// </summary>
public sealed class PaymentCompletePurchaseRequestHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PaymentCompletePurchaseRequestHandler> _logger;

    public PaymentCompletePurchaseRequestHandler(
        IApplicationDbContext context,
        ILogger<PaymentCompletePurchaseRequestHandler> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await this._context.Payments
                .AsNoTracking()
                .Where(p => p.Id == notification.PaymentId)
                .Select(p => new { p.ItemId, p.BuyerId, p.BundleId })
                .FirstOrDefaultAsync(cancellationToken);
            if (payment is null || payment.ItemId is null || payment.BundleId is not null) return;

            var request = await this._context.PurchaseRequests
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.ItemId == payment.ItemId
                                       && r.BuyerId == payment.BuyerId
                                       && r.Status == PurchaseRequestStatus.Accepted,
                                       cancellationToken);
            if (request is null) return;

            request.Status = PurchaseRequestStatus.Completed;
            request.Item.IsActive = false;
            request.Item.IsReserved = false;
            request.Item.IsSold = true;
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to complete purchase request for payment {PaymentId}", notification.PaymentId);
        }
    }
}
