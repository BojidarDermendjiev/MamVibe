using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

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
        IConfiguration? config = null,
        Mock<IShippingService>? shippingMock = null,
        Mock<IEBillService>? eBillMock = null,
        Mock<ITakeANapService>? takeANapMock = null)
    {
        shippingMock ??= new Mock<IShippingService>();
        eBillMock ??= new Mock<IEBillService>();
        takeANapMock ??= new Mock<ITakeANapService>();
        var webhookMock = new Mock<IN8nWebhookService>();
        var n8nOptions = Options.Create(new N8nSettings());

        return new PaymentService(
            db,
            CreateMapper(),
            config ?? CreateConfig(),
            takeANapMock.Object,
            webhookMock.Object,
            n8nOptions,
            shippingMock.Object,
            eBillMock.Object,
            NullLogger<PaymentService>.Instance);
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

        result.Should().HaveCount(1);
        result[0].BuyerId.Should().Be("buyer-1");
        result[0].Amount.Should().Be(30m);
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

        result.Should().HaveCount(1);
        result[0].SellerId.Should().Be("seller-1");
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

        result.Should().BeEmpty();
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

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeOnOrAfter(result[1].CreatedAt);
    }

    // =========================================================================
    // CreateCheckoutSessionAsync (test mode — no real Stripe key)
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSessionAsync_TestMode_Creates_Payment_Record()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db, price: 45m);

        var svc = CreateService(db, CreateConfig(stripeKey: ""));
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
        var svc = CreateService(db);

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

        var svc = CreateService(db);
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
    public async Task CreateCheckoutSessionAsync_TestMode_Completes_Accepted_PurchaseRequest()
    {
        await using var db = CreateDb();
        var item = await SeedSellItemAsync(db);

        var pr = new PurchaseRequest
        {
            ItemId = item.Id,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Status = PurchaseRequestStatus.Accepted
        };
        db.PurchaseRequests.Add(pr);
        await db.SaveChangesAsync();

        var svc = CreateService(db, CreateConfig(stripeKey: ""));
        await svc.CreateCheckoutSessionAsync(item.Id, "buyer-1", "https://app/success", "https://app/cancel");

        var updated = await db.PurchaseRequests.FindAsync(pr.Id);
        updated!.Status.Should().Be(PurchaseRequestStatus.Completed);
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

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeOnOrAfter(result[1].CreatedAt);
    }
}
