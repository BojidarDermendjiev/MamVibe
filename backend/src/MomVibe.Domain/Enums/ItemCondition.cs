namespace MomVibe.Domain.Enums;

/// <summary>
/// Standardized four-tier condition scale for marketplace item listings.
/// </summary>
public enum ItemCondition
{
    /// <summary>No condition specified (legacy items or seller chose not to specify).</summary>
    Unspecified = 0,

    /// <summary>Brand new, unused, original tags/packaging still attached.</summary>
    NewWithTags = 1,

    /// <summary>Used once or twice; no visible signs of wear.</summary>
    LikeNew = 2,

    /// <summary>Clearly used; normal wear, no damage.</summary>
    Good = 3,

    /// <summary>Visible wear or minor imperfections, as described in the listing.</summary>
    Fair = 4,
}
