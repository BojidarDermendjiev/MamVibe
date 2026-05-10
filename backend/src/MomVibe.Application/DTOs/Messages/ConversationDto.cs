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
    /// <summary>Gets or sets the identifier of the other participant in the conversation.</summary>
    public required string UserId { get; set; }

    /// <summary>Gets or sets the display name of the other participant.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the other participant, or <c>null</c> if not set.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the preview text of the most recent message in the conversation.</summary>
    public required string LastMessage { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the most recent message.</summary>
    public DateTime LastMessageTime { get; set; }

    /// <summary>Gets or sets the number of unread messages from the other participant.</summary>
    public int UnreadCount { get; set; }
}
