namespace MomVibe.Domain.Enums;

/// <summary>
/// Distinguishes item listings as donation or sale.
/// </summary>
public enum ListingType
{
    /// <summary>
    /// Listing offered free of charge.
    /// </summary>
    Donate = 0,

    /// <summary>
    /// Listing offered for a price.
    /// </summary>
    Sell = 1
}
