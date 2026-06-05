namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a direct message between two users, with content and read state.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Message : BaseEntity
{
    /// <summary>
    /// Identifier of the sending user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string SenderId { get; set; }

    /// <summary>
    /// Identifier of the receiving user (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string ReceiverId { get; set; }

    /// <summary>
    /// Textual content of the message.
    /// </summary>
    [Required]
    [MinLength(MessageConstants.Lengths.ContentMin)]
    [MaxLength(MessageConstants.Lengths.ContentMax)]
    public required string Content { get; set; }

    /// <summary>
    /// Indicates whether the message has been read by the receiver.
    /// </summary>
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