namespace MomVibe.Application.Interfaces;

using Domain.Entities;

/// <summary>
/// Defines operations for integrating with the TakeANap payment provider,
/// which handles cash-on-delivery order creation for marketplace transactions.
/// </summary>
public interface ITakeANapService
{
    /// <summary>
    /// Creates an order with the TakeANap provider for the given payment and
    /// returns the provider-issued receipt URL or reference.
    /// </summary>
    /// <param name="payment">The payment record for which to create the order.</param>
    /// <returns>The receipt URL or reference string, or <c>null</c> if the provider did not return one.</returns>
    Task<string?> CreateOrderAndGetReceiptAsync(Payment payment);
}
