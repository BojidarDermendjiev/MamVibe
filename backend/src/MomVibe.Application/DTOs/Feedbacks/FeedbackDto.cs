namespace MomVibe.Application.DTOs.Feedbacks;

using Domain.Enums;

/// <summary>
/// DTO representing a submitted feedback entry:
/// - Identity: Id.
/// - Author: UserId, UserDisplayName, UserAvatarUrl.
/// - Content: Rating, Category, Content, IsContactable.
/// - Metadata: CreatedAt timestamp.
/// </summary>
public class FeedbackDto
{
    /// <summary>Gets or sets the unique identifier of the feedback entry.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who submitted the feedback.</summary>
    public required string UserId { get; set; }

    /// <summary>Gets or sets the display name of the submitting user, or <c>null</c> if not available.</summary>
    public string? UserDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the submitting user, or <c>null</c> if not set.</summary>
    public string? UserAvatarUrl { get; set; }

    /// <summary>Gets or sets the numeric rating from 1 (lowest) to 5 (highest).</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the category classifying the type of feedback.</summary>
    public FeedbackCategory Category { get; set; }

    /// <summary>Gets or sets the textual content of the feedback.</summary>
    public required string Content { get; set; }

    /// <summary>Gets or sets a value indicating whether the user consents to being contacted for follow-up.</summary>
    public bool IsContactable { get; set; }

    /// <summary>Gets or sets the UTC date and time when the feedback was submitted.</summary>
    public DateTime CreatedAt { get; set; }
}
