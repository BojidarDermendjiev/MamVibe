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
    public Guid Id { get; set; }
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public string? SenderDisplayName { get; set; }
    public string? SenderAvatarUrl { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}
