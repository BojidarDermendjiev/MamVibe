namespace MomVibe.Domain.Enums;

/// <summary>
/// Specifies how a shipment is delivered to the recipient.
/// </summary>
public enum DeliveryType
{
    /// <summary>
    /// Delivery to a courier office for pickup.
    /// </summary>
    Office = 0,

    /// <summary>
    /// Delivery to a specific street address.
    /// </summary>
    Address = 1,

    /// <summary>
    /// Delivery to an automated parcel locker.
    /// </summary>
    Locker = 2
}
