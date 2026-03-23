namespace MomVibe.Domain.Enums;

/// <summary>
/// Age group the item is suitable for.
/// </summary>
public enum AgeGroup
{
    /// <summary>0 – 3 months.</summary>
    Newborn = 0,

    /// <summary>3 – 12 months.</summary>
    Infant = 1,

    /// <summary>1 – 3 years.</summary>
    Toddler = 2,

    /// <summary>3 – 5 years.</summary>
    Preschool = 3,

    /// <summary>5 – 12 years.</summary>
    SchoolAge = 4,

    /// <summary>12+ years.</summary>
    Teen = 5,
}
