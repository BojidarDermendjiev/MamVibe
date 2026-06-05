namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a user's like on an item. Enforces uniqueness per (UserId, ItemId).
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Like : BaseEntity
{
    /// <summary>
    /// Identifier of the user who liked the item (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>
    /// Foreign key referencing the liked item.
    /// </summary>
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