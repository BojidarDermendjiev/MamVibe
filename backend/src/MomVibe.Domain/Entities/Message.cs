namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a direct message between two users, with content and read state.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - EF Core <see cref="CommentAttribute"/> provides descriptive database column comments.
/// - Indexes support common query patterns (by participants, read state, creation time).
/// </remarks>
[Index(nameof(SenderId))]
[Index(nameof(ReceiverId))]
[Index(nameof(IsRead))]
[Index(nameof(CreatedAt))]
[Index(nameof(SenderId), nameof(ReceiverId), nameof(CreatedAt))] // conversation ordering
public class Message : BaseEntity
{
    /// <summary>
    /// Identifier of the sending user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(MessageConstants.Comments.SenderId)]
    public required string SenderId { get; set; }

    /// <summary>
    /// Identifier of the receiving user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    [Comment(MessageConstants.Comments.ReceiverId)]
    public required string ReceiverId { get; set; }

    /// <summary>
    /// Textual content of the message.
    /// </summary>
    [Required]
    [MinLength(MessageConstants.Lengths.ContentMin)]
    [MaxLength(MessageConstants.Lengths.ContentMax)]
    [Comment(MessageConstants.Comments.Content)]
    public required string Content { get; set; }

    /// <summary>
    /// Indicates whether the message has been read by the receiver.
    /// </summary>
    [Comment(MessageConstants.Comments.IsRead)]
    public bool IsRead { get; set; } = MessageConstants.Defaults.IsRead;

    /// <summary>
    /// Navigation to the sending user.
    /// </summary>
    public ApplicationUser Sender { get; set; } = null!;

    /// <summary>
    /// Navigation to the receiving user.
    /// </summary>
    public ApplicationUser Receiver { get; set; } = null!;
}