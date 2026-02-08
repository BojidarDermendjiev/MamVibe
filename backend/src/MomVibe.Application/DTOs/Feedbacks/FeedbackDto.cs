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
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public int Rating { get; set; }
    public FeedbackCategory Category { get; set; }
    public required string Content { get; set; }
    public bool IsContactable { get; set; }
    public DateTime CreatedAt { get; set; }
}
