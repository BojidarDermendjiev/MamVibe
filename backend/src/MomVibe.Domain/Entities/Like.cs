namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a user's like on an item. Enforces uniqueness per (UserId, ItemId).
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Composite unique index prevents duplicate likes by the same user on the same item.
/// - Column comments aid database documentation.
/// </remarks>
[Index(nameof(UserId))]
[Index(nameof(ItemId))]
[Index(nameof(UserId), nameof(ItemId), IsUnique = true)]
public class Like : BaseEntity
{
    /// <summary>
    /// Identifier of the user who liked the item (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(LikeConstants.Comments.UserId)]
    public required string UserId { get; set; }

    /// <summary>
    /// Foreign key referencing the liked item.
    /// </summary>
    [Comment(LikeConstants.Comments.ItemId)]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Navigation to the user who liked the item.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Navigation to the liked item.
    /// </summary>
    public Item Item { get; set; } = null!;
}