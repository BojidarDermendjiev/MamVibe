using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using MomVibe.Application.Events;
using MomVibe.Application.Interfaces;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="StripePaymentService"/>.
///
/// Stripe SDK static helpers (EventUtility.ConstructEvent, SessionService, PaymentIntentService)
/// call out to Stripe APIs and cannot be mocked without an interface seam.  The tests below
/// exercise every code path that is reachable without a live Stripe connection:
///
///  1. CreatePaymentIntentAsync — when Stripe is unconfigured the method returns a well-known
///     simulated secret without calling the Stripe API.
///  2. CreateCheckoutSessionAsync — test-mode path creates a Payment row with Completed status
///     and publishes PaymentCompletedEvent, which is the same observable effect as the real
///     CheckoutSessionCompleted webhook in production.
///  3. HandleWebhookAsync — a missing WebhookSecret configuration causes an early
///     InvalidOperationException before the Stripe SDK is ever reached.
///  4. Self-purchase guard — documents that the guard is not yet implemented.
/// </summary>
public class StripePaymentServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"StripeTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>Creates a configuration with no Stripe keys so the service falls back to test mode.</summary>
    private static IConfiguration CreateUnconfiguredStripeConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // No Stripe:SecretKey — IsStripeConfigured() returns false
                ["Stripe:WebhookSecret"] = string.Empty
            })
            .Build();

    /// <summary>Creates a configuration with a webhook secret but no valid Stripe key.</summary>
    private static IConfiguration CreateConfigWithWebhookSecret(string secret) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stripe:SecretKey"] = string.Empty,   // no real key
                ["Stripe:WebhookSecret"] = secret
            })
            .Build();

    private static StripePaymentService CreateService(
        ApplicationDbContext db,
        IConfiguration? config = null,
        Mock<IPublisher>? publisherMock = null,
        Mock<IShippingService>? shippingMock = null)
    {
        config        ??= CreateUnconfiguredStripeConfig();
        publisherMock ??= new Mock<IPublisher>();
        shippingMock  ??= new Mock<IShippingService>();

        return new StripePaymentService(
            db,
            config,
            shippingMock.Object,
            publisherMock.Object,
            NullLogger<StripePaymentService>.Instance);
    }

    private static ApplicationUser SeedUser(ApplicationDbContext db, string id = "user-seller-1")
    {
        var user = new ApplicationUser
        {
            Id          = id,
            DisplayName = "Test Seller",
            UserName    = $"{id}@test.com",
            Email       = $"{id}@test.com",
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    private static Item SeedSellItem(ApplicationDbContext db, string sellerId, decimal price = 25.00m)
    {
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category
        {
            Id   = categoryId,
            Name = "Test Category",
            Slug = $"test-{categoryId}"
        });

        var item = new Item
        {
            Id          = Guid.NewGuid(),
            Title       = "Test Item",
            Description = "A test item for unit tests",
            UserId      = sellerId,
            CategoryId  = categoryId,
            Price       = price,
            ListingType = ListingType.Sell,
            IsActive    = true
        };
        db.Items.Add(item);
        db.SaveChanges();
        return item;
    }

    // =========================================================================
    // Test 1 — CreatePaymentIntentAsync surfaces a clear error when Stripe is unconfigured
    //
    // Unlike the Checkout flow (which can return a fake success URL in test mode),
    // PaymentIntent is consumed by Stripe Elements on the frontend, which validates
    // the client-secret format and rejects placeholders. Returning a sentinel string
    // would crash Stripe.js with a confusing IntegrationError, so the service throws
    // StripeNotConfiguredException — the controller maps it to HTTP 503.
    // =========================================================================

    [Fact]
    public async Task CreatePaymentIntentAsync_Throws_When_Stripe_Unconfigured()
    {
        // Arrange
        using var db      = CreateDb();
        var seller        = SeedUser(db, "seller-1");
        var item          = SeedSellItem(db, seller.Id, price: 49.99m);
        var service       = CreateService(db); // no Stripe key → unconfigured

        // Act
        var act = () => service.CreatePaymentIntentAsync(item.Id, "buyer-1");

        // Assert
        await act.Should().ThrowAsync<MomVibe.Application.Exceptions.StripeNotConfiguredException>(
            because: "Stripe Elements cannot accept a placeholder client secret");
    }

    // =========================================================================
    // Test 2 — HandleWebhookAsync / payment succeeded path (via checkout test mode)
    //
    // The real HandleWebhookAsync verifies a Stripe HMAC signature which is not
    // injectable without an SDK seam.  Instead we test the equivalent observable
    // effect: CreateCheckoutSessionAsync in test mode creates a Payment row with
    // Status=Completed and publishes PaymentCompletedEvent — precisely the same
    // side-effects the webhook handler produces in production.
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSession_TestMode_Marks_Payment_Completed_And_Publishes_Event()
    {
        // Arrange
        using var db        = CreateDb();
        var seller          = SeedUser(db, "seller-2");
        var item            = SeedSellItem(db, seller.Id, price: 19.99m);
        var publisherMock   = new Mock<IPublisher>();
        var service         = CreateService(db, publisherMock: publisherMock);

        // Act
        var url = await service.CreateCheckoutSessionAsync(
            item.Id,
            buyerId:    "buyer-2",
            successUrl: "https://app.test/success",
            cancelUrl:  "https://app.test/cancel");

        // Assert — a Payment row with Completed status was persisted
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.ItemId == item.Id);
        payment.Should().NotBeNull(because: "a payment record must be created for a test-mode checkout");
        payment!.Status.Should().Be(PaymentStatus.Completed,
            because: "test-mode immediately completes the payment without a real Stripe flow");
        payment.BuyerId.Should().Be("buyer-2");
        payment.SellerId.Should().Be(seller.Id);

        // Assert — PaymentCompletedEvent was published (downstream handlers update item status)
        publisherMock.Verify(
            p => p.Publish(It.Is<PaymentCompletedEvent>(e => e.PaymentId == payment.Id), It.IsAny<CancellationToken>()),
            Times.Once,
            "downstream event handlers rely on this event to mark the item as sold");
    }

    // =========================================================================
    // Test 3 — HandleWebhookAsync with missing webhook secret throws early
    // =========================================================================

    [Fact]
    public async Task HandleWebhookAsync_MissingWebhookSecret_Throws_InvalidOperationException()
    {
        // Arrange — webhook secret is intentionally absent from config
        using var db   = CreateDb();
        var config     = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Stripe:WebhookSecret is missing entirely
            })
            .Build();
        var service    = CreateService(db, config: config);

        // Act
        var act = () => service.HandleWebhookAsync("{}", "t=0,v1=bad");

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>(
                because: "the service must refuse to process any webhook if the secret is not configured, " +
                         "preventing signature bypass attacks");
    }

    /// <summary>
    /// Variant that also covers a webhook with an invalid Stripe signature string.
    /// When the secret IS configured the Stripe SDK itself throws StripeException for a
    /// forged/malformed signature — this test documents the contract and serves as a
    /// regression guard if the service's early-return logic changes.
    /// </summary>
    [Fact]
    public async Task HandleWebhookAsync_InvalidSignature_Throws()
    {
        // Arrange — provide a real-looking webhook secret so the service proceeds to SDK verification
        using var db   = CreateDb();
        var config     = CreateConfigWithWebhookSecret("whsec_test_secret_1234567890abcdef");
        var service    = CreateService(db, config: config);

        // Act — pass a syntactically invalid signature (not a real Stripe HMAC)
        var act = () => service.HandleWebhookAsync("{}", "t=bad,v1=notvalid");

        // Assert — must throw; Stripe SDK raises StripeException for invalid signatures
        await act.Should()
            .ThrowAsync<Exception>(
                because: "a forged or malformed signature must never be accepted; " +
                         "the Stripe SDK raises StripeException which should propagate to the caller");
    }

    // =========================================================================
    // Test 4 — Self-purchase guard
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSessionAsync_ThrowsIfSellerBuysOwnItem()
    {
        using var db   = CreateDb();
        var seller     = SeedUser(db, "seller-self");
        var item       = SeedSellItem(db, sellerId: seller.Id);
        var svc        = CreateService(db);

        var act = () => svc.CreateCheckoutSessionAsync(
            item.Id, buyerId: seller.Id,
            successUrl: "https://app/success",
            cancelUrl:  "https://app/cancel");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot purchase your own item*");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ThrowsIfSellerBuysOwnItem()
    {
        using var db   = CreateDb();
        var seller     = SeedUser(db, "seller-self-2");
        var item       = SeedSellItem(db, sellerId: seller.Id);
        var svc        = CreateService(db);

        var act = () => svc.CreatePaymentIntentAsync(item.Id, buyerId: seller.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot purchase your own item*");
    }
}
