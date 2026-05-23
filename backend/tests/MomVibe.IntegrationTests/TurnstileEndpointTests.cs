using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Application.DTOs.Turnstile;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Tests for TurnstileController using a factory that stubs ITurnstileService.
/// The stub verifies all tokens except "invalid-token".
/// </summary>
public class TurnstileEndpointTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TurnstileEndpointTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Verify_WithValidToken_Returns200WithVerifiedTrue()
    {
        var request = new TurnstileVerifyRequestDto { Token = "valid-token-abc" };
        var response = await _client.PostAsJsonAsync("/api/turnstile/verify", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<VerifyResponse>();
        body!.Verified.Should().BeTrue();
    }

    [Fact]
    public async Task Verify_WithInvalidToken_Returns400()
    {
        var request = new TurnstileVerifyRequestDto { Token = "invalid-token" };
        var response = await _client.PostAsJsonAsync("/api/turnstile/verify", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record VerifyResponse(bool Verified);
}
