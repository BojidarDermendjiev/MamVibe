using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for TurnstileService which verifies Cloudflare Turnstile CAPTCHA tokens.
/// HttpClient is mocked via Moq.Protected on HttpMessageHandler so no real network calls are made.
/// </summary>
public class TurnstileServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private const string RealSecretKey = "real-secret-key-xyz";
    private const string TestSecretKey = "1x0000000000000000000000000000000AA"; // starts with Cloudflare test prefix

    private static TurnstileService CreateService(
        HttpStatusCode statusCode,
        string responseJson,
        string secretKey = RealSecretKey)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://challenges.cloudflare.com/")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Cloudflare:TurnstileSecretKey", secretKey }
            })
            .Build();

        return new TurnstileService(factoryMock.Object, config);
    }

    // =========================================================================
    // VerifyTokenAsync — happy path
    // =========================================================================

    [Fact]
    public async Task VerifyTokenAsync_Returns_True_When_Cloudflare_Responds_Success_True()
    {
        var svc = CreateService(HttpStatusCode.OK, """{"success":true}""");

        var result = await svc.VerifyTokenAsync("valid-token");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyTokenAsync_Returns_False_When_Cloudflare_Responds_Success_False()
    {
        var svc = CreateService(HttpStatusCode.OK, """{"success":false}""");

        var result = await svc.VerifyTokenAsync("bad-token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTokenAsync_Throws_JsonException_On_Malformed_Json_Response()
    {
        // The production VerifyTokenAsync calls JsonSerializer.Deserialize without a try/catch,
        // so invalid JSON propagates as a JsonException to the caller.
        var svc = CreateService(HttpStatusCode.OK, "not-json-at-all");

        var act = async () => await svc.VerifyTokenAsync("any-token");

        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    [Fact]
    public async Task VerifyTokenAsync_Returns_False_On_Empty_Json_Object()
    {
        // success field absent → defaults to false
        var svc = CreateService(HttpStatusCode.OK, "{}");

        var result = await svc.VerifyTokenAsync("any-token");

        result.Should().BeFalse();
    }

    // =========================================================================
    // VerifyTokenAsync — Cloudflare test secret key auto-pass
    // =========================================================================

    [Fact]
    public async Task VerifyTokenAsync_Returns_True_Without_Http_Call_For_Test_Secret_Key()
    {
        // A mock that always fails — if it's called the test would fail.
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("HTTP should not be called for test keys"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://challenges.cloudflare.com/")
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Cloudflare:TurnstileSecretKey", TestSecretKey }
            })
            .Build();

        var svc = new TurnstileService(factoryMock.Object, config);

        var result = await svc.VerifyTokenAsync("any-token");

        result.Should().BeTrue();
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // =========================================================================
    // VerifyAsync (with IP) — happy path and error handling
    // =========================================================================

    [Fact]
    public async Task VerifyAsync_Returns_True_When_Response_Is_Success_True()
    {
        var svc = CreateService(HttpStatusCode.OK, """{"success":true}""");

        var result = await svc.VerifyAsync("valid-token", "1.2.3.4");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAsync_Returns_False_When_Response_Is_Success_False()
    {
        var svc = CreateService(HttpStatusCode.OK, """{"success":false}""");

        var result = await svc.VerifyAsync("invalid-token", "1.2.3.4");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_Throws_When_Http_Returns_Non_Success_Status()
    {
        var svc = CreateService(HttpStatusCode.InternalServerError, "{}");

        // EnsureSuccessStatusCode() is called on VerifyAsync so it throws HttpRequestException
        var act = async () => await svc.VerifyAsync("token", "1.2.3.4");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // =========================================================================
    // Constructor guard
    // =========================================================================

    [Fact]
    public void Constructor_Throws_InvalidOperation_When_SecretKey_Not_Configured()
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()) // no key
            .Build();

        var act = () => new TurnstileService(factoryMock.Object, config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TurnstileSecretKey*");
    }
}
