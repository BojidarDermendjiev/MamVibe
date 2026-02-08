namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
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
/// - EF Core <see cref="CommentAttribute"/> provides descriptive database column comments.
/// - Indexes on <c>UserId</c> and <c>Category</c> aid common query patterns.
/// </remarks>
[Index(nameof(UserId))]
[Index(nameof(Category))]
public class Feedback : BaseEntity
{
    /// <summary>
    /// Identifier of the user who submitted the feedback (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(FeedbackConstants.Comments.UserId)]
    public required string UserId { get; set; }

    /// <summary>
    /// Feedback rating from 1 (lowest) to 5 (highest).
    /// </summary>
    [Range(FeedbackConstants.Range.RatingMin, FeedbackConstants.Range.RatingMax)]
    [Comment(FeedbackConstants.Comments.Rating)]
    public int Rating { get; set; }

    /// <summary>
    /// Category/type of the feedback (e.g., bug, feature request, general).
    /// </summary>
    [Comment(FeedbackConstants.Comments.Category)]
    public FeedbackCategory Category { get; set; }

    /// <summary>
    /// Textual content of the feedback.
    /// </summary>
    [Required]
    [MinLength(FeedbackConstants.Lengths.ContentMin)]
    [MaxLength(FeedbackConstants.Lengths.ContentMax)]
    [Comment(FeedbackConstants.Comments.Content)]
    public required string Content { get; set; }

    /// <summary>
    /// Whether the user consents to being contacted regarding this feedback.
    /// </summary>
    [Comment(FeedbackConstants.Comments.IsContactable)]
    public bool IsContactable { get; set; }

    /// <summary>
    /// Navigation to the submitting user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
