namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// Optional delivery information attached to a payment request.
/// When provided, the payment service will automatically create a courier shipment.
/// </summary>
public class PaymentDeliveryRequest
{
    public CourierProvider CourierProvider { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public decimal Weight { get; set; } = 1m;
}
