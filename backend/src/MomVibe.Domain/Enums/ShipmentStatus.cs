namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a shipment from creation to final delivery or cancellation.
/// </summary>
public enum ShipmentStatus
{
    /// <summary>
    /// Shipment record created but not yet submitted to the courier.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Shipment created with the courier and waybill issued.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Package picked up by the courier from the sender.
    /// </summary>
    PickedUp = 2,

    /// <summary>
    /// Package in transit between courier facilities.
    /// </summary>
    InTransit = 3,

    /// <summary>
    /// Package out for final delivery to the recipient.
    /// </summary>
    OutForDelivery = 4,

    /// <summary>
    /// Package successfully delivered to the recipient.
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// Package returned to the sender.
    /// </summary>
    Returned = 6,

    /// <summary>
    /// Shipment cancelled before delivery.
    /// </summary>
    Cancelled = 7
}
