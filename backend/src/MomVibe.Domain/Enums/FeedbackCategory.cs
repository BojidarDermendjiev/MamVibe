namespace MomVibe.Domain.Enums;

/// <summary>
/// Categorizes user-submitted feedback to aid triage, routing, and prioritization.
/// </summary>
public enum FeedbackCategory
{
    /// <summary>
    /// Positive feedback highlighting what works well.
    /// </summary>
    Praise = 0,
   
    /// <summary>
    /// Suggestions to improve existing features or user experience.
    /// </summary>
    Improvement = 1,

    /// <summary>
    /// Requests for new features or capabilities not currently available.
    /// </summary>
    FeatureRequest = 2,

    /// <summary>
    /// Reports of errors, defects, or malfunctions.
    /// </summary>
    BugReport = 3
}
