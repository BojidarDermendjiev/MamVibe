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
    Task<List<ConversationDto>> GetConversationsAsync(string userId);
    Task<List<MessageDto>> GetMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50);
    Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content);
    Task MarkAsReadAsync(string userId, string senderId);

    /// <summary>
    /// Generates an AI reply to the user's latest message and saves it as a message from the AI bot.
    /// Returns null if the AI call fails (must never throw).
    /// </summary>
    Task<MessageDto?> SendAiResponseAsync(string userId, string userMessage);
}
