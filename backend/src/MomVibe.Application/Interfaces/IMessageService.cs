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
}
