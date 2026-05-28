namespace MomVibe.Application.DTOs.Follows;

public class NewFollowerNotification
{
    public required string FollowerId { get; set; }
    public required string FollowerDisplayName { get; set; }
    public string? FollowerAvatarUrl { get; set; }
    public DateTime FollowedAt { get; set; }
}
