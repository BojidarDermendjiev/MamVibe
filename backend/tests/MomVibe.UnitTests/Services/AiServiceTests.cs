using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

public class AiServiceTests
{
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

    private static Mock<IChatClient> CreateClientMock(string replyText)
    {
        var mock = new Mock<IChatClient>();
        mock.Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, replyText)));
        return mock;
    }

    private static (AiListingService Service, Mock<IChatClient> ClientMock) CreateListingService(
        string replyText, ApplicationDbContext? db = null)
    {
        db ??= CreateDb();
        var clientMock = CreateClientMock(replyText);
        var settings = Options.Create(new AnthropicSettings { ApiKey = "test-key", Model = "claude-haiku-4-5-20251001" });
        return (new AiListingService(settings, db, clientMock.Object), clientMock);
    }

    private static (AiModerationService Service, Mock<IChatClient> ClientMock) CreateModerationService(
        string replyText, ApplicationDbContext? db = null)
    {
        db ??= CreateDb();
        var clientMock = CreateClientMock(replyText);
        var settings = Options.Create(new AnthropicSettings { ApiKey = "test-key", Model = "claude-haiku-4-5-20251001" });

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns("C:\\fake\\wwwroot");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "R2:PublicUrl", "https://r2.example.com" } })
            .Build();

        return (new AiModerationService(settings, db, clientMock.Object, envMock.Object, config), clientMock);
    }

    private static AiService CreateService(string replyText, ApplicationDbContext? db = null)
    {
        db ??= CreateDb();
        var clientMock = CreateClientMock(replyText);
        var settings     = Options.Create(new AnthropicSettings { ApiKey = "test-key",      Model = "claude-haiku-4-5-20251001" });
        var groqSettings = Options.Create(new GroqSettings      { ApiKey = "groq-test-key", Model = "llama-3.3-70b-versatile" });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "AI:ChatProvider", "anthropic" } })
            .Build();
        return new AiService(settings, groqSettings, db, new FakeKeyedServiceProvider(clientMock.Object), config);
    }

    private static Mock<IFormFile> CreatePhotoMock(string contentType = "image/jpeg", int sizeBytes = 1024)
    {
        var bytes  = new byte[sizeBytes];
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
        var innerJson = """{"title":"Baby Romper","description":"Soft cotton romper.","categorySlug":"clothing","listingType":1,"suggestedPrice":25,"ageGroup":1,"clothingSize":74,"shoeSize":null}""";
        var (svc, _) = CreateListingService(innerJson);

        var result = await svc.SuggestListingAsync(CreatePhotoMock().Object);

        result.Should().NotBeNull();
        result.Title.Should().Be("Baby Romper");
        result.CategorySlug.Should().Be("clothing");
        result.ListingType.Should().Be(ListingType.Sell);
        result.SuggestedPrice.Should().Be(25m);
        result.ClothingSize.Should().Be(74);
        result.ShoeSize.Should().BeNull();
    }

    [Fact]
    public async Task SuggestListingAsync_Returns_Default_On_Malformed_Json_Response()
    {
        var (svc, _) = CreateListingService("this is not json at all");

        var result = await svc.SuggestListingAsync(CreatePhotoMock().Object);

        result.Should().NotBeNull();
        result.Title.Should().BeEmpty();
        result.CategorySlug.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestListingAsync_Throws_InvalidOperation_For_Oversized_Photo()
    {
        var (svc, _) = CreateListingService("{}");

        var photo = CreatePhotoMock(sizeBytes: 6 * 1024 * 1024);
        var act   = async () => await svc.SuggestListingAsync(photo.Object);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*5 MB*");
    }

    [Fact]
    public async Task SuggestListingAsync_Throws_InvalidOperation_For_Unsupported_ContentType()
    {
        var (svc, _) = CreateListingService("{}");

        var photo = CreatePhotoMock(contentType: "image/gif");
        var act   = async () => await svc.SuggestListingAsync(photo.Object);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Unsupported image format*");
    }

    // =========================================================================
    // ModerateItemAsync
    // =========================================================================

    [Fact]
    public async Task ModerateItemAsync_Returns_Approve_Decision_On_Valid_Response()
    {
        var innerJson = """{"recommendation":"approve","confidence":0.95,"reason":"Great item.","flags":[]}""";
        var (svc, _)  = CreateModerationService(innerJson);

        var result = await svc.ModerateItemAsync("Baby stroller", "Excellent condition.", "Strollers", ListingType.Sell, price: 200m);

        result.Recommendation.Should().Be("approve");
        result.Confidence.Should().BeApproximately(0.95, 0.001);
        result.Flags.Should().BeEmpty();
    }

    [Fact]
    public async Task ModerateItemAsync_Returns_Reject_Decision_With_Flags()
    {
        var innerJson = """{"recommendation":"reject","confidence":0.99,"reason":"Spam detected.","flags":["spam","contact-info"]}""";
        var (svc, _)  = CreateModerationService(innerJson);

        var result = await svc.ModerateItemAsync("Buy now call 0888", "Call me at 0888123456.", "Other", ListingType.Sell, price: 1m);

        result.Recommendation.Should().Be("reject");
        result.Flags.Should().Contain("spam").And.Contain("contact-info");
    }

    [Fact]
    public async Task ModerateItemAsync_Returns_Review_Default_On_Malformed_Response()
    {
        var (svc, _) = CreateModerationService("{{totally invalid}}");

        var result = await svc.ModerateItemAsync("Title", "Desc", "Cat", ListingType.Sell, null);

        result.Recommendation.Should().Be("review");
        result.Confidence.Should().Be(0.5);
    }

    // =========================================================================
    // SuggestPriceAsync
    // =========================================================================

    [Fact]
    public async Task SuggestPriceAsync_Returns_Parsed_Price_On_Valid_Response()
    {
        var innerJson = """{"suggestedPrice":45,"low":30,"high":60,"confidence":0.85,"reason":"Based on 5 similar items."}""";
        var (svc, _)  = CreateListingService(innerJson);

        var result = await svc.SuggestPriceAsync(
            "Baby jacket", "Warm winter jacket, size 80.", "Clothing",
            AgeGroup.Infant, clothingSize: 80, shoeSize: null,
            comparablePrices: [40m, 45m, 50m]);

        result.SuggestedPrice.Should().Be(45m);
        result.Low.Should().Be(30m);
        result.High.Should().Be(60m);
        result.Confidence.Should().BeApproximately(0.85, 0.001);
        result.ComparableCount.Should().Be(3);
    }

    [Fact]
    public async Task SuggestPriceAsync_Returns_Default_On_Malformed_Response()
    {
        var (svc, _) = CreateListingService("definitely not json");

        var result = await svc.SuggestPriceAsync("T", "D", "C", null, null, null, []);

        result.SuggestedPrice.Should().BeNull();
        result.Confidence.Should().Be(0.5);
        result.ComparableCount.Should().Be(0);
    }

    // =========================================================================
    // ChatAsync
    // =========================================================================

    [Fact]
    public async Task ChatAsync_Returns_Reply_String_From_Provider()
    {
        var svc = CreateService("AI reply from mock provider");

        var result = await svc.ChatAsync(
            "You are a helpful assistant.",
            [("user", "Hello!")]);

        result.Should().Be("AI reply from mock provider");
    }

    // =========================================================================
    // FakeKeyedServiceProvider
    // =========================================================================

    private sealed class FakeKeyedServiceProvider : IServiceProvider, IKeyedServiceProvider
    {
        private readonly IChatClient _client;

        public FakeKeyedServiceProvider(IChatClient client) => _client = client;

        public object? GetService(Type serviceType) =>
            serviceType == typeof(IChatClient) ? _client : null;

        public object? GetKeyedService(Type serviceType, object? serviceKey) =>
            serviceType == typeof(IChatClient) ? _client : null;

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey) =>
            serviceType == typeof(IChatClient)
                ? _client
                : throw new InvalidOperationException($"No keyed service for {serviceType}");
    }
}
