namespace MomVibe.Application.Interfaces;

using DTOs.PurchaseRequests;

/// <summary>
/// Business logic for buyer purchase/reservation requests.
/// </summary>
public interface IPurchaseRequestService
{
    /// <summary>
    /// Creates a new request for the given item, atomically locking it
    /// (setting IsActive = false) so no other buyer can claim it simultaneously.
    /// Throws <see cref="KeyNotFoundException"/> if the item does not exist and
    /// <see cref="InvalidOperationException"/> if it is already reserved.
    /// </summary>
    Task<PurchaseRequestDto> CreateAsync(Guid itemId, string buyerId);

    /// <summary>
    /// Seller accepts the pending request.
    /// For Donate items a Booking payment is created immediately.
    /// For Sell items the request moves to Accepted, awaiting buyer payment choice.
    /// </summary>
    Task<PurchaseRequestDto> AcceptAsync(Guid requestId, string sellerId);

    /// <summary>
    /// Seller declines the pending request. The item is returned to the shop (IsActive = true).
    /// </summary>
    Task<PurchaseRequestDto> DeclineAsync(Guid requestId, string sellerId);

    /// <summary>
    /// Notifies the seller that the buyer has chosen a payment method for an accepted request.
    /// Does not create the payment — the existing payment endpoints handle that.
    /// </summary>
    Task<PurchaseRequestDto> NotifyPaymentChosenAsync(Guid requestId, string buyerId, string paymentMethod);

    /// <summary>Returns all purchase requests where the caller is the seller.</summary>
    Task<List<PurchaseRequestDto>> GetAsSellerAsync(string sellerId);

    /// <summary>Returns all purchase requests where the caller is the buyer.</summary>
    Task<List<PurchaseRequestDto>> GetAsBuyerAsync(string buyerId);

    /// <summary>
    /// Checks the buyer's reputation on nekorekten.com.
    /// Only the seller of the given request may call this.
    /// Returns <see cref="BuyerCheckResult.ServiceUnavailable"/> when the external API is unreachable.
    /// </summary>
    Task<BuyerCheckResult> CheckBuyerAsync(Guid requestId, string sellerId);
}
