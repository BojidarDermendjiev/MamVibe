namespace MomVibe.Application.DTOs.Messages;

/// <summary>
/// DTO for a conversation entry in the user's inbox:
/// - UserId: identifier of the other participant.
/// - DisplayName: other participant's display name.
/// - AvatarUrl: optional profile image URL.
/// - LastMessage: preview text of the most recent message.
/// - LastMessageTime: timestamp of the last message.
/// - UnreadCount: number of unread messages from the other user.
/// </summary>
public class ConversationDto
{
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public required string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
}
