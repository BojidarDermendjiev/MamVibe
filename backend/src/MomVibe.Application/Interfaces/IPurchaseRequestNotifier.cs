namespace MomVibe.Application.Interfaces;

using DTOs.PurchaseRequests;

/// <summary>
/// Abstraction for pushing real-time purchase-request notifications to connected clients.
/// Implemented in the WebApi layer using SignalR so that the Infrastructure layer
/// does not depend on MomVibe.WebApi.
/// </summary>
public interface IPurchaseRequestNotifier
{
    /// <summary>Pushes a new purchase request notification to the seller.</summary>
    Task NotifySellerAsync(string sellerId, PurchaseRequestDto request);

    /// <summary>Pushes a status-update notification (Accepted/Declined) to the buyer.</summary>
    Task NotifyBuyerAsync(string buyerId, PurchaseRequestDto request);

    /// <summary>
    /// Pushes a PaymentMethodChosen notification to the seller after the buyer
    /// selects their payment method for an accepted request.
    /// </summary>
    Task NotifyPaymentChosenAsync(string sellerId, Guid requestId, string paymentMethod, string buyerName);
}
