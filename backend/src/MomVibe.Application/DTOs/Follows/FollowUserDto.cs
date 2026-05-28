namespace MomVibe.Application.DTOs.Follows;

public class FollowUserDto
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsOnHoliday { get; set; }
    public int FollowerCount { get; set; }
    public int ItemCount { get; set; }
    public DateTime FollowedAt { get; set; }
}
