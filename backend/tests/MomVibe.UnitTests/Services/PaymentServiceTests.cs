using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.DTOs.Payments;
using MomVibe.Application.DTOs.Shipping;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for PaymentService using EF Core InMemory.
/// Stripe calls are bypassed via an empty/missing key so the service
/// executes its test-mode branch. External deps are Moq mocks.
/// TransactionIgnoredWarning is suppressed (InMemory does not support transactions).
/// </summary>
public class PaymentServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PaymentTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    /// <summary>
    /// Configuration without a real Stripe key so the service uses test-mode branch.
    /// </summary>
    private static IConfiguration CreateConfig(string? stripeKey = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stripe:SecretKey"] = stripeKey ?? "",
                ["Stripe:WebhookSecret"] = "whsec_test"
            })
            .Build();

    private static PaymentService CreateService(
        ApplicationDbContext db,
        Mock<IShippingService>? shippingMock = null,
        Mock<IPublisher>? publisherMock = null)
    {
        shippingMock ??= new Mock<IShippingService>();
        publisherMock ??= new Mock<IPublisher>();

        return new PaymentService(
            db,
            CreateMapper(),
            shippingMock.Object,
            publisherMock.Object,
            NullLogger<PaymentService>.Instance);
    }

    private static StripePaymentService CreateStripeService(
        ApplicationDbContext db,
        IConfiguration? config = null,
        Mock<IShippingService>? shippingMock = null,
        Mock<IPublisher>? publisherMock = null)
    {
        shippingMock ??= new Mock<IShippingService>();
        publisherMock ??= new Mock<IPublisher>();

        return new StripePaymentService(
            db,
            config ?? CreateConfig(),
            shippingMock.Object,
            publisherMock.Object,
            NullLogger<StripePaymentService>.Instance);
    }

    /// <summary>Seeds a seller, buyer, and sellable item; returns the item.</summary>
    private static async Task<Item> SeedSellItemAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        decimal price = 30m)
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser { Id = sellerId, DisplayName = "Seller", Email = "seller@test.com", UserName = sellerId });
        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser { Id = buyerId, DisplayName = "Buyer", Email = "buyer@test.com", UserName = buyerId });

        var item = new Item
        {
            Title = "Baby Onesie",
            Description = "Soft cotton onesie",
            UserId = sellerId,
            ListingType = ListingType.Sell,
            Price = price,
            IsActive = true
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    /// <summary>Seeds a seller, buyer, and donate item; returns the item.</summary>
    private static async Task<Item> SeedDonateItemAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1")
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser { Id = sellerId, DisplayName = "Seller", Email = "seller@test.com", UserName = sellerId });
        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser { Id = buyerId, DisplayName = "Buyer", Email = "buyer@test.com", UserName = buyerId });

        var item = new Item
        {
            Title = "Donated Toy",
            Description = "Free toy in good condition",
            UserId = sellerId,
            ListingType = ListingType.Donate,
            Price = null,
            IsActive = true
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    // =========================================================================
    // GetPaymentsByUserAsync
    // =========================================================================

    [Fact]
    public async Task GetPaymentsByUserAsync_Returns_Payments_Where_User_Is_Buyer()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        db.Payments.Add(new Payment
        {
            ItemId = item.Id,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Amount = 30m,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.Completed
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetPaymentsByUserAsync("buyer-1");

        result.Items.Should().HaveCount(1);
        result.Items[0].BuyerId.Should().Be("buyer-1");
        result.Items[0].Amount.Should().Be(30m);
    }

    [Fact]
    public async Task GetPaymentsByUserAsync_Returns_Payments_Where_User_Is_Seller()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        db.Payments.Add(new Payment
        {
            ItemId = item.Id,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Amount = 30m,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.Completed
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetPaymentsByUserAsync("seller-1");

        result.Items.Should().HaveCount(1);
        result.Items[0].SellerId.Should().Be("seller-1");
    }

    [Fact]
    public async Task GetPaymentsByUserAsync_Returns_Empty_For_Unrelated_User()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        db.Payments.Add(new Payment
        {
            ItemId = item.Id,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Amount = 30m,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.Completed
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetPaymentsByUserAsync("unrelated-user");

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaymentsByUserAsync_Returns_Ordered_By_CreatedAt_Descending()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        // Add two payments; EF InMemory preserves insertion order but we verify ordering
        db.Payments.Add(new Payment { ItemId = item.Id, BuyerId = "buyer-1", SellerId = "seller-1", Amount = 10m, PaymentMethod = PaymentMethod.Card, Status = PaymentStatus.Completed });
        db.Payments.Add(new Payment { ItemId = item.Id, BuyerId = "buyer-1", SellerId = "seller-1", Amount = 20m, PaymentMethod = PaymentMethod.Card, Status = PaymentStatus.Completed });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetPaymentsByUserAsync("buyer-1");

        result.Items.Should().HaveCount(2);
        result.Items[0].CreatedAt.Should().BeOnOrAfter(result.Items[1].CreatedAt);
    }

    // =========================================================================
    // CreateCheckoutSessionAsync (test mode — no real Stripe key)
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSessionAsync_TestMode_Creates_Payment_Record()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 45m);

        var svc = CreateStripeService(db, CreateConfig(stripeKey: ""));
        var result = await svc.CreateCheckoutSessionAsync(
            item.Id, "buyer-1",
            "https://app/success", "https://app/cancel");

        result.Should().Contain("test_simulated");

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.ItemId == item.Id && p.BuyerId == "buyer-1");
        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(45m);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_Throws_KeyNotFound_For_Missing_Item()
    {
        await using var db = CreateDb();
        var svc = CreateStripeService(db);

        var act = async () => await svc.CreateCheckoutSessionAsync(
            Guid.NewGuid(), "buyer-1",
            "https://app/success", "https://app/cancel");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found.");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_Throws_InvalidOperation_For_Donate_Item()
    {
        await using var db = CreateDb();
        var item = await SeedDonateItemAsync(db);

        var svc = CreateStripeService(db);
        var act = async () => await svc.CreateCheckoutSessionAsync(
            item.Id, "buyer-1",
            "https://app/success", "https://app/cancel");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // CreateBookingAsync
    // =========================================================================

    [Fact]
    public async Task CreateBookingAsync_Saves_Payment_With_Booking_Method_And_Zero_Amount()
    {
        await using var db = CreateDb();
        var item = await SeedDonateItemAsync(db);

        var svc = CreateService(db);
        var result = await svc.CreateBookingAsync(item.Id, "buyer-1");

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be(PaymentMethod.Booking);
        result.Amount.Should().Be(0m);
        result.Status.Should().Be(PaymentStatus.Completed);

        var persisted = await db.Payments.FirstOrDefaultAsync(p => p.ItemId == item.Id && p.BuyerId == "buyer-1");
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_Throws_KeyNotFound_For_Missing_Item()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.CreateBookingAsync(Guid.NewGuid(), "buyer-1");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found.");
    }

    [Fact]
    public async Task CreateBookingAsync_Throws_InvalidOperation_For_Non_Donate_Item()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        var svc = CreateService(db);
        var act = async () => await svc.CreateBookingAsync(item.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*donate*");
    }

    // =========================================================================
    // CreateOnSpotPaymentAsync
    // =========================================================================

    [Fact]
    public async Task CreateOnSpotPaymentAsync_Saves_Payment_With_OnSpot_Method_And_Pending_Status()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 50m);

        var svc = CreateService(db);
        var result = await svc.CreateOnSpotPaymentAsync(item.Id, "buyer-1");

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be(PaymentMethod.OnSpot);
        result.Status.Should().Be(PaymentStatus.Pending);
        result.Amount.Should().Be(50m);

        var persisted = await db.Payments.FirstOrDefaultAsync(p => p.ItemId == item.Id && p.BuyerId == "buyer-1");
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOnSpotPaymentAsync_Throws_KeyNotFound_For_Missing_Item()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.CreateOnSpotPaymentAsync(Guid.NewGuid(), "buyer-1");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found.");
    }

    // =========================================================================
    // CreateCheckoutSessionAsync — PurchaseRequest completion
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSessionAsync_TestMode_Publishes_PaymentCompletedEvent()
    {
        // Purchase-request completion now lives in PaymentCompletePurchaseRequestHandler
        // (see Infrastructure.EventHandlers). At the PaymentService unit-test boundary
        // we verify the event is published — the handler's behaviour is covered separately.
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        var publisher = new Mock<MediatR.IPublisher>();
        var svc = CreateStripeService(db, CreateConfig(stripeKey: ""), publisherMock: publisher);
        await svc.CreateCheckoutSessionAsync(item.Id, "buyer-1", "https://app/success", "https://app/cancel");

        publisher.Verify(p => p.Publish(
            It.Is<MomVibe.Application.Events.PaymentCompletedEvent>(e => e.IsTestMode),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================================
    // CreateCashOnDeliveryAsync
    // =========================================================================

    private static PaymentDeliveryRequest CreateDelivery() => new()
    {
        RecipientName = "Test User",
        RecipientPhone = "+359888000000",
        CourierProvider = CourierProvider.Econt,
        DeliveryType = DeliveryType.Office,
        OfficeId = "OFF-1",
        Weight = 1m
    };

    [Fact]
    public async Task CreateCashOnDeliveryAsync_Throws_KeyNotFound_For_Missing_Item()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.CreateCashOnDeliveryAsync(Guid.NewGuid(), "buyer-1", CreateDelivery());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found.");
    }

    [Fact]
    public async Task CreateCashOnDeliveryAsync_Throws_InvalidOperation_For_Donate_Item()
    {
        await using var db = CreateDb();
        var item = await SeedDonateItemAsync(db);
        var svc = CreateService(db);

        var act = async () => await svc.CreateCashOnDeliveryAsync(item.Id, "buyer-1", CreateDelivery());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateCashOnDeliveryAsync_Saves_Payment_With_CashOnDelivery_Method_And_Pending_Status()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 30m);
        var svc = CreateService(db);

        var result = await svc.CreateCashOnDeliveryAsync(item.Id, "buyer-1", CreateDelivery());

        result.PaymentMethod.Should().Be(PaymentMethod.CashOnDelivery);
        result.Status.Should().Be(PaymentStatus.Pending);

        var persisted = await db.Payments.FirstOrDefaultAsync(p => p.ItemId == item.Id && p.BuyerId == "buyer-1");
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCashOnDeliveryAsync_Amount_Includes_Shipping_Fee()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 30m);

        var shippingMock = new Mock<IShippingService>();
        shippingMock.Setup(s => s.CalculatePriceAsync(It.IsAny<CalculateShippingDto>()))
            .ReturnsAsync(new ShippingPriceResultDto { Price = 8m, Currency = "BGN" });

        var svc = CreateService(db, shippingMock: shippingMock);

        var result = await svc.CreateCashOnDeliveryAsync(item.Id, "buyer-1", CreateDelivery());

        result.Amount.Should().Be(38m); // 30 item + 8 shipping
    }

    [Fact]
    public async Task CreateCashOnDeliveryAsync_CodAmount_Equals_Item_Price_Plus_Shipping()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 30m);

        var shippingMock = new Mock<IShippingService>();
        shippingMock.Setup(s => s.CalculatePriceAsync(It.IsAny<CalculateShippingDto>()))
            .ReturnsAsync(new ShippingPriceResultDto { Price = 8m, Currency = "BGN" });

        CreateShipmentDto? captured = null;
        shippingMock.Setup(s => s.CreateShipmentAsync(It.IsAny<CreateShipmentDto>()))
            .Callback<CreateShipmentDto>(dto => captured = dto)
            .ReturnsAsync(new ShipmentDto { RecipientName = "Test User", RecipientPhone = "+359888000000" });

        var svc = CreateService(db, shippingMock: shippingMock);
        await svc.CreateCashOnDeliveryAsync(item.Id, "buyer-1", CreateDelivery());

        captured.Should().NotBeNull();
        captured!.IsCod.Should().BeTrue();
        captured.CodAmount.Should().Be(38m); // courier collects item + shipping
    }

    [Fact]
    public async Task CreateCashOnDeliveryAsync_Shipping_Failure_Falls_Back_To_Item_Price_Only()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 30m);

        var shippingMock = new Mock<IShippingService>();
        shippingMock.Setup(s => s.CalculatePriceAsync(It.IsAny<CalculateShippingDto>()))
            .ThrowsAsync(new Exception("Courier unavailable"));

        var svc = CreateService(db, shippingMock: shippingMock);
        var result = await svc.CreateCashOnDeliveryAsync(item.Id, "buyer-1", CreateDelivery());

        result.Amount.Should().Be(30m); // graceful fallback: item price only
    }

    // =========================================================================
    // GetAllPaymentsAsync
    // =========================================================================

    [Fact]
    public async Task GetAllPaymentsAsync_Returns_All_Payments_Ordered_Descending()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        db.Payments.Add(new Payment { ItemId = item.Id, BuyerId = "buyer-1", SellerId = "seller-1", Amount = 5m, PaymentMethod = PaymentMethod.Card, Status = PaymentStatus.Completed });
        db.Payments.Add(new Payment { ItemId = item.Id, BuyerId = "buyer-1", SellerId = "seller-1", Amount = 15m, PaymentMethod = PaymentMethod.OnSpot, Status = PaymentStatus.Pending });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetAllPaymentsAsync();

        result.Items.Should().HaveCount(2);
        result.Items[0].CreatedAt.Should().BeOnOrAfter(result.Items[1].CreatedAt);
    }
}
