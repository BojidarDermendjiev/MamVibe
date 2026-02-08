namespace MomVibe.Application.DTOs.Shipping;

using Domain.Enums;

/// <summary>
/// DTO for creating a new shipment:
/// - PaymentId: links to the payment transaction.
/// - CourierProvider and DeliveryType: courier and delivery method selection.
/// - Recipient: name and phone of the recipient.
/// - Delivery: address/city for Address delivery, OfficeId/OfficeName for Office/Locker delivery.
/// - Financial: weight, COD toggle and amount, insurance toggle and amount.
/// </summary>
public class CreateShipmentDto
{
    public Guid PaymentId { get; set; }
    public CourierProvider CourierProvider { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public required string RecipientName { get; set; }
    public required string RecipientPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? City { get; set; }
    public string? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public decimal Weight { get; set; }
    public bool IsCod { get; set; }
    public decimal CodAmount { get; set; }
    public bool IsInsured { get; set; }
    public decimal InsuredAmount { get; set; }
}
