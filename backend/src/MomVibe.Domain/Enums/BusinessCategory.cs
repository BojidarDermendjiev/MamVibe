namespace MomVibe.Domain.Enums;

/// <summary>
/// Top-level discriminator separating the two flavours of paying business profiles on
/// the platform. Drives which public browse page surfaces the profile's listing
/// (<c>/coaches</c> vs <c>/venues</c>) and is used by the admin queue for category-level filtering.
/// </summary>
public enum BusinessCategory
{
    /// <summary>
    /// A coach, instructor, or agency offering activities for children
    /// (swimming, martial arts, music, dance, gymnastics, etc.). Listings appear on <c>/coaches</c>.
    /// </summary>
    Coach = 0,

    /// <summary>
    /// A venue, attraction, or place operator advertising their physical location to families
    /// (indoor playgrounds, family-friendly restaurants, museums, parks). Listings appear on <c>/venues</c>.
    /// </summary>
    VenueAdvertiser = 1
}
