namespace MomVibe.Application.DTOs.Feedbacks;

using Domain.Enums;

/// <summary>
/// DTO for creating user feedback:
/// - Rating: numeric score (e.g., 1–5).
/// - Category: classification of feedback.
/// - Content: required textual message.
/// - IsContactable: whether the user consents to being contacted for follow-up.
/// </summary>
public class CreateFeedbackDto
{
    public int Rating { get; set; }
    public FeedbackCategory Category { get; set; }
    public required string Content { get; set; }
    public bool IsContactable { get; set; }
}
