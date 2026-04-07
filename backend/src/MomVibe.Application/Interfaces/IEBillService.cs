namespace MomVibe.Application.Interfaces;

using DTOs.Payments;

/// <summary>
/// Provides access to the buyer's e-bills (electronic payment receipts).
/// Only completed purchases of Sell-type items produce an e-bill.
/// Donations and on-spot payments are excluded.
/// </summary>
public interface IEBillService
{
    /// <summary>
    /// Assigns an <c>EBillNumber</c> to the payment (if not yet assigned) and sends the
    /// receipt email to <paramref name="buyerEmail"/>. Idempotent — a second call for the
    /// same payment skips both steps. Safe to call fire-and-forget inside a catch block.
    /// </summary>
    Task IssueEBillAsync(Guid paymentId, string buyerEmail);

    /// <summary>Returns all e-bills for the given buyer, newest first.</summary>
    Task<List<EBillDto>> GetMyEBillsAsync(string userId);

    /// <summary>Returns a single e-bill. Throws <see cref="UnauthorizedAccessException"/> if the payment does not belong to <paramref name="userId"/>.</summary>
    Task<EBillDto> GetEBillAsync(Guid paymentId, string userId);

    /// <summary>Re-sends the receipt email to the buyer's registered address. Fire-and-forget on TakeANap/SMTP errors.</summary>
    Task ResendEBillEmailAsync(Guid paymentId, string userId);
}
