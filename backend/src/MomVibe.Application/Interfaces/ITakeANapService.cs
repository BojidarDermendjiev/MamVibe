namespace MomVibe.Application.Interfaces;

using Domain.Entities;

public interface ITakeANapService
{
    Task<string?> CreateOrderAndGetReceiptAsync(Payment payment);
}
