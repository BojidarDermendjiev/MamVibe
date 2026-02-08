namespace MomVibe.Domain.Enums;

/// <summary>
/// Identifies the courier service provider used for shipment delivery.
/// </summary>
public enum CourierProvider
{
    /// <summary>
    /// Econt Express courier service.
    /// </summary>
    Econt = 0,

    /// <summary>
    /// Speedy courier service.
    /// </summary>
    Speedy = 1,

    /// <summary>
    /// Box Now locker/office delivery service.
    /// </summary>
    BoxNow = 2
}
