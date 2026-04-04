namespace MomVibe.Application.Interfaces;

using Domain.Entities;

/// <summary>
/// Service for creating digital fiscal receipts via the Take a NAP API.
/// Receipts are generated automatically for all taxable credit events — marketplace
/// payments and wallet transactions alike — without any manual reporting step.
/// </summary>
public interface ITakeANapService
{
    /// <summary>
    /// Creates a fiscal receipt for a completed marketplace item payment.
    /// </summary>
    Task<string?> CreateOrderAndGetReceiptAsync(Payment payment);

    /// <summary>
    /// Creates a fiscal receipt for a wallet credit event (top-up, transfer received, refund).
    /// The customer email is passed explicitly because wallet transactions are not directly
    /// linked to a Payment entity.
    /// Only call for Credit-type transactions — debit events do not require a receipt.
    /// </summary>
    Task<string?> CreateWalletReceiptAsync(WalletTransaction transaction, string customerEmail);
}
