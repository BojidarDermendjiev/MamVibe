namespace MomVibe.Domain.Entities;

using Common;

/// <summary>
/// Daily rollup of activity for a <see cref="BusinessListing"/>. Aggregated nightly from
/// <see cref="BusinessListingViewEvent"/> + counter deltas so the business dashboard
/// can render long-range charts without scanning the raw events table.
/// </summary>
public class BusinessListingDailyStat : BaseEntity
{
    /// <summary>FK to the listing the stats belong to.</summary>
    public Guid ListingId { get; set; }

    /// <summary>The calendar day (UTC midnight) these counters cover.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Total view events recorded on this day.</summary>
    public long Views { get; set; }

    /// <summary>Distinct viewers (computed from <see cref="BusinessListingViewEvent.ViewerHash"/>).</summary>
    public long UniqueViewers { get; set; }

    /// <summary>Net likes added on this day (likes minus unlikes).</summary>
    public long Likes { get; set; }

    /// <summary>Comments posted on this day (not counting hidden ones).</summary>
    public long Comments { get; set; }

    /// <summary>Navigation to the listing.</summary>
    public BusinessListing Listing { get; set; } = null!;
}
