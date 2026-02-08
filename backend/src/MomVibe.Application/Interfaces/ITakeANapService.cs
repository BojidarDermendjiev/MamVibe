namespace MomVibe.Application.Interfaces;

using Domain.Entities;

/// <summary>
/// Service for creating digital receipts via Take a NAP API.
/// </summary>
public interface ITakeANapService
{
    Task<string?> CreateOrderAndGetReceiptAsync(Payment payment);
}
