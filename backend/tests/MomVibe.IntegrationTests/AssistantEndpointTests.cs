using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Application.DTOs.Assistant;

namespace MomVibe.IntegrationTests;

public class AssistantPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AssistantPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Chat_WithoutAuth_Returns401()
    {
        var request = new AssistantChatRequest { Message = "How do I sell items?" };
        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class AssistantAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AssistantAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Chat_WithEmptyMessage_Returns400()
    {
        var request = new AssistantChatRequest { Message = "" };
        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_WithWhitespaceMessage_Returns400()
    {
        var request = new AssistantChatRequest { Message = "   " };
        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_WithMessageExceeding600Chars_Returns400()
    {
        var request = new AssistantChatRequest { Message = new string('a', 601) };
        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_WithValidMessage_Returns200WithReply()
    {
        var request = new AssistantChatRequest
        {
            Message = "How do I sell items on MamVibe?",
            Language = "en",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        body!.Reply.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Chat_WithConversationHistory_Returns200()
    {
        var request = new AssistantChatRequest
        {
            Message = "And how do I buy?",
            Language = "en",
            History = [new AssistantMessage { Role = "user", Content = "Hello" }, new AssistantMessage { Role = "assistant", Content = "Hi there!" }]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Chat_WithExactly600Chars_Returns200()
    {
        var request = new AssistantChatRequest { Message = new string('a', 600) };
        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =========================================================================
    // Off-topic guard (end-to-end — no LLM call, instant rejection)
    // =========================================================================

    [Fact]
    public async Task Chat_WithOffTopicPoem_Returns200_WithEnglishRejection()
    {
        var request = new AssistantChatRequest
        {
            Message = "write me a poem about flowers",
            Language = "en",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        body!.Reply.Should().Be("I can only help with questions about the MamVibe platform.");
    }

    [Fact]
    public async Task Chat_WithOffTopicPoem_BulgarianLanguage_Returns200_WithBulgarianRejection()
    {
        var request = new AssistantChatRequest
        {
            Message = "write me a poem",
            Language = "bg",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        // Bulgarian rejection — must not be the English string
        body!.Reply.Should().NotBe("I can only help with questions about the MamVibe platform.");
        body.Reply.Should().Contain("MamVibe");
    }

    [Theory]
    [InlineData("tell me a joke")]
    [InlineData("what is the weather in Sofia today")]
    [InlineData("who is the president of Bulgaria")]
    [InlineData("what is the capital of France")]
    [InlineData("show me the latest news")]
    public async Task Chat_WithVariousOffTopicMessages_Returns200_WithRejection(string offTopicMessage)
    {
        var request = new AssistantChatRequest { Message = offTopicMessage, Language = "en" };

        var response = await _client.PostAsJsonAsync("/api/v1/assistant/chat", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        body!.Reply.Should().Contain("MamVibe platform");
    }

    private record ChatResponse(string Reply);
}
