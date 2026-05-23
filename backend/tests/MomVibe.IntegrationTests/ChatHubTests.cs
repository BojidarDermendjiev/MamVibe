using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Thin integration smoke-tests for the ChatHub endpoint.
/// Business-logic (validation, routing, presence) is covered by unit tests in
/// MomVibe.UnitTests/Hubs/ChatHubTests.cs using mocked SignalR infrastructure.
///
/// Note: Full LongPolling e2e tests require a real Kestrel server; TestServer does
/// not support the streaming transport reliably.
/// </summary>
public class ChatHubPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatHubPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Negotiate_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync("/hubs/chat/negotiate?negotiateVersion=1", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class ChatHubAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatHubAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Negotiate_WithAuth_Returns200WithConnectionId()
    {
        var response = await _client.PostAsync("/hubs/chat/negotiate?negotiateVersion=1", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<NegotiateResponse>();
        body.Should().NotBeNull();
        body!.ConnectionId.Should().NotBeNullOrEmpty();
    }

    private record NegotiateResponse(string ConnectionId, string? Url);
}
