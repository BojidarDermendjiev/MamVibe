namespace MomVibe.Domain.Enums;

/// <summary>
/// Category of children's extracurricular activity offered by a <c>BusinessListing</c>.
/// Numeric gaps are intentional so new categories (e.g., STEM/Robotics) can be added
/// without renumbering existing rows.
/// </summary>
public enum ActivityType
{
    /// <summary>Swimming lessons and water-based programs.</summary>
    Swimming = 0,

    /// <summary>Martial arts disciplines (karate, judo, taekwondo, BJJ, etc.).</summary>
    MartialArts = 1,

    /// <summary>Music instruction (instrument lessons, vocal coaching, music theory).</summary>
    Music = 2,

    /// <summary>Dance disciplines (ballet, modern, hip-hop, folk).</summary>
    Dance = 3,

    /// <summary>Gymnastics and acrobatic training.</summary>
    Gymnastics = 4,

    /// <summary>Visual arts and crafts workshops (drawing, painting, ceramics).</summary>
    ArtAndCrafts = 5,

    /// <summary>Early-development programs targeting infants and toddlers (0–3 yrs).</summary>
    EarlyDevelopment = 6,

    /// <summary>Foreign-language and reading-readiness classes.</summary>
    LanguageClasses = 7,

    /// <summary>Team sports (football, basketball, volleyball, hockey).</summary>
    SportsTeam = 8,

    /// <summary>Activities that do not fit any of the standard categories.</summary>
    Other = 99
}
