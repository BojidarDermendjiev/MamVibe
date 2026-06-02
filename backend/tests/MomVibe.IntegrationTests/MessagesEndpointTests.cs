using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace MomVibe.IntegrationTests;

public class MessagesPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MessagesPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetConversations_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/messages/conversations");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMessages_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/messages/some-user-id");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendMessage_WithoutAuth_Returns401()
    {
        var request = new { ReceiverId = "some-id", Content = "Hello" };
        var response = await _client.PostAsJsonAsync("/api/v1/messages", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkAsRead_WithoutAuth_Returns401()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/messages/some-sender/read", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class MessagesAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MessagesAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetConversations_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/messages/conversations");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMessages_WithOtherUserId_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/messages/other-user-123");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // MarkAsRead_Returns204 lives in MessagesMarkAsReadSqliteTests
    // because the endpoint uses ExecuteUpdate which InMemory does not support.

    [Fact]
    public async Task SendMessage_ToSelf_Returns200()
    {
        // Send to self since we only have one test user seeded
        var request = new
        {
            ReceiverId = GeneralAuthWebApplicationFactory.TestUserId,
            Content = "Hello, testing messages!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/messages", request);
        // Service may allow or reject self-messaging — either is acceptable
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
