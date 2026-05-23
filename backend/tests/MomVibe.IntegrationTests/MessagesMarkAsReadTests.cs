using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MomVibe.Application.DTOs.Messages;
using MomVibe.Application.Interfaces;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Factory that inherits the full GeneralAuthWebApplicationFactory setup (InMemory DB, auth scheme,
/// seeded test user) and replaces IMessageService with a no-op stub so that MarkAsReadAsync
/// does not hit ExecuteUpdate — which InMemory does not support.
/// </summary>
public class MessagesMarkAsReadWebApplicationFactory : GeneralAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessageService));
            if (descriptor != null) services.Remove(descriptor);
            services.AddScoped<IMessageService, NoOpMessageService>();
        });
    }
}

/// <summary>
/// No-op IMessageService stub: all methods return empty/null so the HTTP layer
/// (routing, auth, status codes) can be tested without a real DB.
/// </summary>
file sealed class NoOpMessageService : IMessageService
{
    public Task<List<ConversationDto>> GetConversationsAsync(string userId) =>
        Task.FromResult(new List<ConversationDto>());

    public Task<List<MessageDto>> GetMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50) =>
        Task.FromResult(new List<MessageDto>());

    public Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content) =>
        Task.FromResult(new MessageDto { SenderId = senderId, ReceiverId = receiverId, Content = content });

    public Task MarkAsReadAsync(string userId, string senderId) => Task.CompletedTask;

    public Task<MessageDto?> SendAiResponseAsync(string userId, string userMessage) =>
        Task.FromResult<MessageDto?>(null);
}

public class MessagesMarkAsReadTests : IClassFixture<MessagesMarkAsReadWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MessagesMarkAsReadTests(MessagesMarkAsReadWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MarkAsRead_Returns204()
    {
        var response = await _client.PutAsJsonAsync("/api/messages/some-sender/read", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
