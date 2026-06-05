namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Represents a user-submitted feedback entry with rating, category, content, and contact consent.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Feedback : BaseEntity
{
    /// <summary>
    /// Identifier of the user who submitted the feedback (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>
    /// Feedback rating from 1 (lowest) to 5 (highest).
    /// </summary>
    [Range(FeedbackConstants.Range.RatingMin, FeedbackConstants.Range.RatingMax)]
    public int Rating { get; set; }

    /// <summary>
    /// Category/type of the feedback (e.g., bug, feature request, general).
    /// </summary>
    public FeedbackCategory Category { get; set; }

    /// <summary>
    /// Textual content of the feedback.
    /// </summary>
    [Required]
    [MinLength(FeedbackConstants.Lengths.ContentMin)]
    [MaxLength(FeedbackConstants.Lengths.ContentMax)]
    public required string Content { get; set; }

    /// <summary>
    /// Whether the user consents to being contacted regarding this feedback.
    /// </summary>
    public bool IsContactable { get; set; }

    /// <summary>
    /// Navigation to the submitting user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
