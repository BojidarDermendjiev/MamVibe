namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="PaymentCompletedEvent"/>, issues an e-bill for the buyer and emails it
/// via <see cref="IEBillService"/>. Failure is logged but never propagated; the buyer
/// can always re-trigger the email from the e-bill list endpoint.
/// </summary>
public sealed class PaymentEBillHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEBillService _eBills;
    private readonly ILogger<PaymentEBillHandler> _logger;

    public PaymentEBillHandler(
        IApplicationDbContext context,
        IEBillService eBills,
        ILogger<PaymentEBillHandler> logger)
    {
        this._context = context;
        this._eBills = eBills;
        this._logger = logger;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.IsTestMode) return;

        try
        {
            var buyerEmail = await this._context.Payments
                .AsNoTracking()
                .Where(p => p.Id == notification.PaymentId)
                .Select(p => p.Buyer!.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(buyerEmail)) return;

            await this._eBills.IssueEBillAsync(notification.PaymentId, buyerEmail);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "E-bill issuance failed for payment {PaymentId}", notification.PaymentId);
        }
    }
}
