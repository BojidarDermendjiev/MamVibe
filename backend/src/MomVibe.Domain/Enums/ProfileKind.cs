namespace MomVibe.Domain.Enums;

/// <summary>
/// Distinguishes a single-instructor business from a multi-instructor organisation
/// when a <c>BusinessProfile</c> registers on the platform.
/// </summary>
public enum ProfileKind
{
    /// <summary>An individual instructor or freelance coach.</summary>
    Coach = 0,

    /// <summary>A studio, school, or multi-instructor organisation.</summary>
    Agency = 1
}
