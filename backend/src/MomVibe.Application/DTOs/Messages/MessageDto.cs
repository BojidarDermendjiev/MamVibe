namespace MomVibe.Application.DTOs.Messages;

/// <summary>
/// DTO representing a single direct message:
/// - Id: unique message identifier.
/// - Sender/Receiver: SenderId, ReceiverId with optional sender display info.
/// - Content: required message body.
/// - Timestamp: when the message was sent.
/// - IsRead: whether the receiver has read the message.
/// </summary>
public class MessageDto
{
    /// <summary>Gets or sets the unique identifier of the message.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who sent the message.</summary>
    public required string SenderId { get; set; }

    /// <summary>Gets or sets the identifier of the user who received the message.</summary>
    public required string ReceiverId { get; set; }

    /// <summary>Gets or sets the display name of the sender, or <c>null</c> if not loaded.</summary>
    public string? SenderDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the sender, or <c>null</c> if not set.</summary>
    public string? SenderAvatarUrl { get; set; }

    /// <summary>Gets or sets the textual content of the message.</summary>
    public required string Content { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the message was sent.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets or sets a value indicating whether the receiver has read the message.</summary>
    public bool IsRead { get; set; }
}
