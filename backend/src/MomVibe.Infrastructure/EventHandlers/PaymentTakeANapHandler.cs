namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Application.Events;
using Application.Interfaces;

/// <summary>
/// On <see cref="PaymentCompletedEvent"/>, creates a digital receipt via Take a NAP and
/// stores the resulting URL on the Payment row. Test-mode payments are skipped — Take a NAP
/// rejects synthetic data and there is nothing to receipt.
/// </summary>
public sealed class PaymentTakeANapHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ITakeANapService _takeANap;
    private readonly ILogger<PaymentTakeANapHandler> _logger;

    public PaymentTakeANapHandler(
        IApplicationDbContext context,
        ITakeANapService takeANap,
        ILogger<PaymentTakeANapHandler> logger)
    {
        this._context = context;
        this._takeANap = takeANap;
        this._logger = logger;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.IsTestMode) return;

        try
        {
            var payment = await this._context.Payments
                .Include(p => p.Item)
                .Include(p => p.Buyer)
                .FirstOrDefaultAsync(p => p.Id == notification.PaymentId, cancellationToken);
            if (payment is null) return;

            var receiptUrl = await this._takeANap.CreateOrderAndGetReceiptAsync(payment);
            if (receiptUrl is null) return;

            payment.ReceiptUrl = receiptUrl;
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Take-a-NAP receipt creation failed for payment {PaymentId}", notification.PaymentId);
        }
    }
}
