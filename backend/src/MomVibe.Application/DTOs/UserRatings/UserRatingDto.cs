namespace MomVibe.Application.DTOs.UserRatings;

public class UserRatingDto
{
    public Guid Id { get; set; }
    public string RaterId { get; set; } = string.Empty;
    public string? RaterDisplayName { get; set; }
    public string? RaterAvatarUrl { get; set; }
    public string RatedUserId { get; set; } = string.Empty;
    public Guid PurchaseRequestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
