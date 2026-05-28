namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Common;

[Index(nameof(FollowerId))]
[Index(nameof(FolloweeId))]
[Index(nameof(FollowerId), nameof(FolloweeId), IsUnique = true)]
public class Follow : BaseEntity
{
    [Required]
    public required string FollowerId { get; set; }

    [Required]
    public required string FolloweeId { get; set; }

    public ApplicationUser Follower { get; set; } = null!;
    public ApplicationUser Followee { get; set; } = null!;
}
