namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

public class Follow : BaseEntity
{
    [Required]
    public required string FollowerId { get; set; }

    [Required]
    public required string FolloweeId { get; set; }

    public ApplicationUser Follower { get; set; } = null!;
    public ApplicationUser Followee { get; set; } = null!;
}
