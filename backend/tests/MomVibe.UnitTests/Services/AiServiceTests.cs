using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

using MomVibe.Application.Interfaces;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for AiService which calls the Anthropic and Groq HTTP APIs.
/// The HttpClient used internally is mocked via Moq.Protected so no real network calls occur.
/// IFormFile is mocked with Moq because the service reads from its ContentType, Length, and stream.
/// IWebHostEnvironment is mocked for tests that exercise photo-fetch code paths.
/// </summary>
public class AiServiceTests
{
    // =========================================================================
    // Constants
    // =========================================================================

    /// <summary>Minimal valid Anthropic API response body with an embedded JSON payload.</summary>
    private static string AnthropicResponse(string innerJson) => $$"""
        {
          "content": [{ "type": "text", "text": "{{innerJson.Replace("\"", "\\\"")}}" }]
        }
        """;

    private static string AnthropicResponseRaw(string innerText) => JsonSerializer.Serialize(new
    {
        content = new[] { new { type = "text", text = innerText } }
    });

    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"AiServiceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (AiService Service, Mock<HttpMessageHandler> Handler) CreateServiceWithHandler(
        HttpStatusCode statusCode,
        string responseBody,
        ApplicationDbContext? db = null)
    {
        db ??= CreateDb();

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
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var anthropicSettings = Options.Create(new AnthropicSettings
        {
            ApiKey = "test-key",
            Model = "claude-haiku-4-5-20251001"
        });
        var groqSettings = Options.Create(new GroqSettings
        {
            ApiKey = "groq-test-key",
            Model = "llama-3.3-70b-versatile"
        });

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns("C:\\fake\\wwwroot");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:ChatProvider", "anthropic" }
            })
            .Build();

        // IServiceProvider for keyed ILlmChatProvider — provide a mock chat provider
        var chatProviderMock = new Mock<ILlmChatProvider>();
        chatProviderMock
            .Setup(p => p.ChatAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<(string role, string content)>>(),
                It.IsAny<string>()))
            .ReturnsAsync("AI reply from mock provider");

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILlmChatProvider)))
            .Returns(chatProviderMock.Object);

        // GetRequiredKeyedService — requires IKeyedServiceProvider; simplest approach:
        // wrap in a fake that implements the keyed interface
        var keyedProvider = new FakeKeyedServiceProvider(chatProviderMock.Object);

        var svc = new AiService(
            anthropicSettings,
            groqSettings,
            db,
            factoryMock.Object,
            envMock.Object,
            config,
            keyedProvider);

        return (svc, handlerMock);
    }

    /// <summary>Creates a minimal mock IFormFile that reads from an in-memory byte array.</summary>
    private static Mock<IFormFile> CreatePhotoMock(
        string contentType = "image/jpeg",
        int sizeBytes = 1024)
    {
        var bytes = new byte[sizeBytes];
        new Random(42).NextBytes(bytes);
        var stream = new MemoryStream(bytes);

        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(sizeBytes);
        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((s, _) => stream.CopyTo(s))
            .Returns(Task.CompletedTask);
        return mock;
    }

    // =========================================================================
    // SuggestListingAsync
    // =========================================================================

    [Fact]
    public async Task SuggestListingAsync_Returns_Parsed_Suggestion_On_Valid_Response()
    {
        var innerJson = """
            {"title":"Baby Romper","description":"Soft cotton romper.","categorySlug":"clothing","listingType":1,"suggestedPrice":25,"ageGroup":1,"clothingSize":74,"shoeSize":null}
            """;
        var body = AnthropicResponseRaw(innerJson);
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var photo = CreatePhotoMock();
        var result = await svc.SuggestListingAsync(photo.Object);

        result.Should().NotBeNull();
        result.Title.Should().Be("Baby Romper");
        result.CategorySlug.Should().Be("clothing");
        result.ListingType.Should().Be(ListingType.Sell);
        result.SuggestedPrice.Should().Be(25m);
        result.ClothingSize.Should().Be(74);
        result.ShoeSize.Should().BeNull();
    }

    [Fact]
    public async Task SuggestListingAsync_Returns_Default_On_Malformed_Json_In_Text()
    {
        // The outer Anthropic envelope is valid but the inner text is not valid JSON.
        // ParseSuggestion has a catch block that returns new AiListingSuggestionDto().
        var body = AnthropicResponseRaw("this is not json at all");
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var photo = CreatePhotoMock();
        var result = await svc.SuggestListingAsync(photo.Object);

        result.Should().NotBeNull();
        result.Title.Should().BeEmpty();
        result.CategorySlug.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestListingAsync_Throws_When_Http_Returns_Error_Status()
    {
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.InternalServerError, "{}");

        var photo = CreatePhotoMock();
        var act = async () => await svc.SuggestListingAsync(photo.Object);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SuggestListingAsync_Throws_InvalidOperation_For_Oversized_Photo()
    {
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, "{}");

        var photo = CreatePhotoMock(sizeBytes: 6 * 1024 * 1024); // 6 MB > 5 MB limit
        var act = async () => await svc.SuggestListingAsync(photo.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*5 MB*");
    }

    [Fact]
    public async Task SuggestListingAsync_Throws_InvalidOperation_For_Unsupported_ContentType()
    {
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, "{}");

        var photo = CreatePhotoMock(contentType: "image/gif");
        var act = async () => await svc.SuggestListingAsync(photo.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unsupported image format*");
    }

    // =========================================================================
    // ModerateItemAsync
    // =========================================================================

    [Fact]
    public async Task ModerateItemAsync_Returns_Approve_Decision_On_Valid_Response()
    {
        var innerJson = """
            {"recommendation":"approve","confidence":0.95,"reason":"Great item.","flags":[]}
            """;
        var body = AnthropicResponseRaw(innerJson);
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var result = await svc.ModerateItemAsync(
            "Baby stroller",
            "Excellent condition stroller.",
            "Strollers",
            ListingType.Sell,
            price: 200m);

        result.Should().NotBeNull();
        result.Recommendation.Should().Be("approve");
        result.Confidence.Should().BeApproximately(0.95, 0.001);
        result.Flags.Should().BeEmpty();
    }

    [Fact]
    public async Task ModerateItemAsync_Returns_Reject_Decision_With_Flags()
    {
        var innerJson = """
            {"recommendation":"reject","confidence":0.99,"reason":"Spam detected.","flags":["spam","contact-info"]}
            """;
        var body = AnthropicResponseRaw(innerJson);
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var result = await svc.ModerateItemAsync(
            "Buy now call 0888",
            "Call me at 0888123456.",
            "Other",
            ListingType.Sell,
            price: 1m);

        result.Recommendation.Should().Be("reject");
        result.Flags.Should().Contain("spam");
        result.Flags.Should().Contain("contact-info");
    }

    [Fact]
    public async Task ModerateItemAsync_Returns_Safe_Default_On_Http_Error()
    {
        // When the HTTP call fails, EnsureSuccessStatusCode throws — the service does not catch it.
        // The safe-default behaviour sits inside ParseModerationResult which is only reached if HTTP succeeds.
        // We verify the exception propagates (callers in ItemService catch it).
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.ServiceUnavailable, "{}");

        var act = async () => await svc.ModerateItemAsync(
            "Title", "Desc", "Cat", ListingType.Sell, null);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ModerateItemAsync_Returns_Review_Default_On_Malformed_Inner_Json()
    {
        // Valid Anthropic envelope, inner text is garbage.
        // ParseModerationResult catches the exception and returns { Recommendation="review", Confidence=0.5 }
        var body = AnthropicResponseRaw("{{totally invalid}}");
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var result = await svc.ModerateItemAsync(
            "Title", "Desc", "Cat", ListingType.Sell, null);

        result.Recommendation.Should().Be("review");
        result.Confidence.Should().Be(0.5);
    }

    // =========================================================================
    // SuggestPriceAsync
    // =========================================================================

    [Fact]
    public async Task SuggestPriceAsync_Returns_Parsed_Price_On_Valid_Response()
    {
        var innerJson = """
            {"suggestedPrice":45,"low":30,"high":60,"confidence":0.85,"reason":"Based on 5 similar items."}
            """;
        var body = AnthropicResponseRaw(innerJson);
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var result = await svc.SuggestPriceAsync(
            "Baby jacket",
            "Warm winter jacket, size 80.",
            "Clothing",
            ageGroup: AgeGroup.Infant,
            clothingSize: 80,
            shoeSize: null,
            comparablePrices: new List<decimal> { 40m, 45m, 50m });

        result.Should().NotBeNull();
        result.SuggestedPrice.Should().Be(45m);
        result.Low.Should().Be(30m);
        result.High.Should().Be(60m);
        result.Confidence.Should().BeApproximately(0.85, 0.001);
        result.ComparableCount.Should().Be(3);
    }

    [Fact]
    public async Task SuggestPriceAsync_Returns_Default_On_Malformed_Inner_Json()
    {
        var body = AnthropicResponseRaw("definitely not json");
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, body);

        var result = await svc.SuggestPriceAsync(
            "T", "D", "C", null, null, null, new List<decimal>());

        result.SuggestedPrice.Should().BeNull();
        result.Confidence.Should().Be(0.5);
        result.ComparableCount.Should().Be(0);
    }

    [Fact]
    public async Task SuggestPriceAsync_Throws_On_Http_Error()
    {
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.TooManyRequests, "{}");

        var act = async () => await svc.SuggestPriceAsync(
            "T", "D", "C", null, null, null, new List<decimal>());

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // =========================================================================
    // ChatAsync
    // =========================================================================

    [Fact]
    public async Task ChatAsync_Returns_Reply_String_From_Provider()
    {
        // ChatAsync delegates to ILlmChatProvider keyed service; the mock returns a fixed reply.
        var (svc, _) = CreateServiceWithHandler(HttpStatusCode.OK, "{}");

        var history = new List<(string role, string content)>
        {
            ("user", "Hello!")
        };

        var result = await svc.ChatAsync("You are a helpful assistant.", history);

        result.Should().Be("AI reply from mock provider");
    }

    // =========================================================================
    // FakeKeyedServiceProvider helper
    // =========================================================================

    /// <summary>
    /// Minimal IServiceProvider + IKeyedServiceProvider implementation used to satisfy
    /// IServiceProvider.GetRequiredKeyedService&lt;ILlmChatProvider&gt;(key) in AiService.ChatAsync.
    /// </summary>
    private sealed class FakeKeyedServiceProvider : IServiceProvider, IKeyedServiceProvider
    {
        private readonly ILlmChatProvider _provider;

        public FakeKeyedServiceProvider(ILlmChatProvider provider) => _provider = provider;

        public object? GetService(Type serviceType) =>
            serviceType == typeof(ILlmChatProvider) ? _provider : null;

        public object? GetKeyedService(Type serviceType, object? serviceKey) =>
            serviceType == typeof(ILlmChatProvider) ? _provider : null;

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey) =>
            serviceType == typeof(ILlmChatProvider)
                ? _provider
                : throw new InvalidOperationException($"No keyed service for {serviceType}");
    }
}
