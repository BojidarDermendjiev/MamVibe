namespace MomVibe.Application.Interfaces;

using DTOs.Messages;

/// <summary>
/// Messaging service contract:
/// - Retrieves conversations for a user (other participant, last message, unread count).
/// - Paginates messages in a thread between two users.
/// - Sends a message and returns the mapped DTO.
/// - Marks messages from a given sender as read for the current user.
/// </summary>
public interface IMessageService
{
    /// <summary>Returns all conversation summaries for the specified user, sorted by most recent activity.</summary>
    /// <param name="userId">The identifier of the user whose conversations to retrieve.</param>
    Task<List<ConversationDto>> GetConversationsAsync(string userId);

    /// <summary>Returns a paginated list of messages in the thread between two users.</summary>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="otherUserId">The identifier of the other participant.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of messages per page.</param>
    Task<List<MessageDto>> GetMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50);

    /// <summary>Sends a message from the sender to the receiver and returns the persisted message DTO.</summary>
    /// <param name="senderId">The identifier of the sending user.</param>
    /// <param name="receiverId">The identifier of the receiving user.</param>
    /// <param name="content">The textual content of the message.</param>
    Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content);

    /// <summary>Marks all unread messages from the specified sender as read for the current user.</summary>
    /// <param name="userId">The identifier of the user reading the messages.</param>
    /// <param name="senderId">The identifier of the user whose messages are being marked as read.</param>
    Task MarkAsReadAsync(string userId, string senderId);

    /// <summary>
    /// Generates an AI reply to the user's latest message and saves it as a message from the AI bot.
    /// Returns null if the AI call fails (must never throw).
    /// </summary>
    Task<MessageDto?> SendAiResponseAsync(string userId, string userMessage);
}
