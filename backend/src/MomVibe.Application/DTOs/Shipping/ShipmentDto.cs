namespace MomVibe.Application.DTOs.Shipping;

using Domain.Enums;

/// <summary>
/// DTO representing a shipment record:
/// - Identity &amp; linkage: Id, PaymentId, optional ItemTitle from Payment.Item.
/// - Courier: CourierProvider, DeliveryType, Status.
/// - Tracking: TrackingNumber, WaybillId.
/// - Recipient: RecipientName, RecipientPhone.
/// - Delivery: DeliveryAddress, City, OfficeId, OfficeName.
/// - Financial: ShippingPrice, IsCod, CodAmount, IsInsured, InsuredAmount.
/// - Package: Weight.
/// - Label: LabelUrl.
/// - Timestamps: CreatedAt.
/// </summary>
public class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string? ItemTitle { get; set; }
    public CourierProvider CourierProvider { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public ShipmentStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? WaybillId { get; set; }
    public required string RecipientName { get; set; }
    public required string RecipientPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? City { get; set; }
    public string? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public decimal ShippingPrice { get; set; }
    public bool IsCod { get; set; }
    public decimal CodAmount { get; set; }
    public bool IsInsured { get; set; }
    public decimal InsuredAmount { get; set; }
    public decimal Weight { get; set; }
    public string? LabelUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
