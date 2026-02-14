namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Application.Interfaces;

/// <summary>
/// Authenticated API controller for direct messaging:
/// - List conversations for the current user
/// - Retrieve paginated messages with another user
/// - Send a new message
/// - Mark messages from a sender as read
/// All endpoints require authentication via the controller-level <c>[Authorize]</c> attribute.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagesController"/>.
    /// </summary>
    /// <param name="messageService">Service handling messaging operations.</param>
    /// <param name="currentUserService">Service providing context about the current user.</param>
    public MessagesController(IMessageService messageService, ICurrentUserService currentUserService)
    {
        this._messageService = messageService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves the list of conversations for the authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with a collection of conversation summaries.
    /// </returns>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var conversations = await this._messageService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    /// <summary>
    /// Retrieves a paginated set of messages between the authenticated user and the specified other user.
    /// </summary>
    /// <param name="otherUserId">The identifier of the other participant.</param>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of messages per page (default: 50).</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with a paged result of messages.
    /// </returns>
    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetMessages(string otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var messages = await this._messageService.GetMessagesAsync(userId, otherUserId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Sends a new message from the authenticated user to the specified receiver.
    /// </summary>
    /// <param name="request">Payload containing the receiver's identifier and the message content.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the created message details on success.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var message = await this._messageService.SendMessageAsync(userId, request.ReceiverId, request.Content);
        return Ok(message);
    }


    /// <summary>
    /// Marks messages from a specific sender as read for the authenticated user.
    /// </summary>
    /// <param name="senderId">The identifier of the sender whose messages should be marked as read.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 204 No Content on success.
    /// </returns>
    [HttpPut("{senderId}/read")]
    public async Task<IActionResult> MarkAsRead(string senderId)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        await this._messageService.MarkAsReadAsync(userId, senderId);
        return NoContent();
    }
}

/// <summary>
/// Request payload for sending a direct message.
/// </summary>
public class SendMessageRequest
{
    /// <summary>Identifier of the message receiver.</summary>
    public required string ReceiverId { get; set; }

    /// <summary>Text content of the message to send.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public required string Content { get; set; }
}
