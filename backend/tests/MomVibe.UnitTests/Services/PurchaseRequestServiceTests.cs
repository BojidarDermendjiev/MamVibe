using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;
using MomVibe.Application.Interfaces;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for PurchaseRequestService using EF Core InMemory.
/// IPurchaseRequestNotifier and INekorektenService are Moq mocks.
/// Users and items are always seeded before PurchaseRequests to avoid the InMemory Include+User navigation bug.
/// TransactionIgnoredWarning is suppressed because InMemory does not support real transactions,
/// but the service uses BeginTransactionAsync in CreateAsync.
/// </summary>
public class PurchaseRequestServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PurchaseRequestTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static PurchaseRequestService CreateService(
        ApplicationDbContext db,
        Mock<IPurchaseRequestNotifier>? notifierMock = null,
        Mock<INekorektenService>? nekorektenMock = null)
    {
        notifierMock ??= new Mock<IPurchaseRequestNotifier>();
        nekorektenMock ??= new Mock<INekorektenService>();
        return new PurchaseRequestService(db, CreateMapper(), notifierMock.Object, nekorektenMock.Object);
    }

    /// <summary>
    /// Seeds a buyer, a seller, and an active item belonging to the seller.
    /// Returns the seeded item. Users are always seeded before the item.
    /// </summary>
    private static async Task<Item> SeedItemAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        ListingType listingType = ListingType.Sell,
        bool isActive = true)
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser
            {
                Id = sellerId,
                DisplayName = "Seller",
                Email = $"{sellerId}@test.com",
                UserName = sellerId
            });

        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser
            {
                Id = buyerId,
                DisplayName = "Buyer",
                Email = $"{buyerId}@test.com",
                UserName = buyerId
            });

        var item = new Item
        {
            Title = "Baby Stroller",
            Description = "Gently used stroller",
            UserId = sellerId,
            IsActive = isActive,
            ListingType = listingType,
            Price = listingType == ListingType.Donate ? null : 80m,
            Photos = [new ItemPhoto { Url = "https://example.com/stroller.jpg", DisplayOrder = 0 }]
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    /// <summary>Seeds an item and a pending PurchaseRequest for it.</summary>
    private static async Task<(Item Item, PurchaseRequest Request)> SeedRequestAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        PurchaseRequestStatus status = PurchaseRequestStatus.Pending,
        ListingType listingType = ListingType.Sell)
    {
        var item = await SeedItemAsync(db, sellerId, buyerId, listingType, isActive: false);

        var request = new PurchaseRequest
        {
            ItemId = item.Id,
            BuyerId = buyerId,
            SellerId = sellerId,
            Status = status
        };
        db.PurchaseRequests.Add(request);
        await db.SaveChangesAsync();
        return (item, request);
    }

    // =========================================================================
    // GetAsBuyerAsync
    // =========================================================================

    [Fact]
    public async Task GetAsBuyerAsync_Returns_Only_Buyer_Requests()
    {
        await using var db = CreateDb();
        await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1");
        await SeedRequestAsync(db, sellerId: "seller-2", buyerId: "buyer-2");

        var svc = CreateService(db);
        var result = await svc.GetAsBuyerAsync("buyer-1");

        result.Should().HaveCount(1);
        result[0].BuyerId.Should().Be("buyer-1");
    }

    [Fact]
    public async Task GetAsBuyerAsync_Returns_Empty_For_New_User()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetAsBuyerAsync("unknown-buyer");
        result.Should().BeEmpty();
    }

    // =========================================================================
    // GetAsSellerAsync
    // =========================================================================

    [Fact]
    public async Task GetAsSellerAsync_Returns_Only_Seller_Item_Requests()
    {
        await using var db = CreateDb();
        await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1");
        await SeedRequestAsync(db, sellerId: "seller-2", buyerId: "buyer-2");

        var svc = CreateService(db);
        var result = await svc.GetAsSellerAsync("seller-1");

        result.Should().HaveCount(1);
        result[0].SellerId.Should().Be("seller-1");
    }

    [Fact]
    public async Task GetAsSellerAsync_Returns_Empty_When_Seller_Has_No_Requests()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetAsSellerAsync("no-requests-seller");
        result.Should().BeEmpty();
    }

    // =========================================================================
    // CreateAsync
    // NOTE: CreateAsync uses ExecuteUpdateAsync (bulk update) with a correlated subquery
    // that EF Core InMemory cannot translate. These tests belong in integration tests
    // (MomVibe.IntegrationTests) where a real SQL provider is available.
    // =========================================================================

    // =========================================================================
    // AcceptAsync
    // =========================================================================

    [Fact]
    public async Task AcceptAsync_Updates_Status_To_Accepted_For_Sell_Item()
    {
        await using var db = CreateDb();
        var (_, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending, listingType: ListingType.Sell);

        var svc = CreateService(db);
        var result = await svc.AcceptAsync(request.Id, "seller-1");

        result.Status.Should().Be(PurchaseRequestStatus.Accepted);

        var updated = await db.PurchaseRequests.FindAsync(request.Id);
        updated!.Status.Should().Be(PurchaseRequestStatus.Accepted);
    }

    [Fact]
    public async Task AcceptAsync_Creates_Booking_Payment_For_Donate_Item()
    {
        await using var db = CreateDb();
        var (_, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending, listingType: ListingType.Donate);

        var svc = CreateService(db);
        await svc.AcceptAsync(request.Id, "seller-1");

        var payment = await db.Payments.FirstOrDefaultAsync(p =>
            p.BuyerId == "buyer-1" && p.SellerId == "seller-1");
        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(0);
        payment.PaymentMethod.Should().Be(PaymentMethod.Booking);
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task AcceptAsync_Throws_Unauthorized_When_Not_The_Seller()
    {
        await using var db = CreateDb();
        var (_, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending);

        var svc = CreateService(db);
        var act = async () => await svc.AcceptAsync(request.Id, "buyer-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not the seller*");
    }

    [Fact]
    public async Task AcceptAsync_Throws_KeyNotFound_For_Missing_Request()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.AcceptAsync(Guid.NewGuid(), "seller-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // DeclineAsync
    // =========================================================================

    [Fact]
    public async Task DeclineAsync_Updates_Status_To_Declined_And_Restores_Item()
    {
        await using var db = CreateDb();
        var (item, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending);

        var svc = CreateService(db);
        var result = await svc.DeclineAsync(request.Id, "seller-1");

        result.Status.Should().Be(PurchaseRequestStatus.Declined);

        // Item should be restored (IsActive = true)
        var restored = await db.Items.FindAsync(item.Id);
        restored!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeclineAsync_Throws_Unauthorized_When_Not_The_Seller()
    {
        await using var db = CreateDb();
        var (_, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending);

        var svc = CreateService(db);
        var act = async () => await svc.DeclineAsync(request.Id, "buyer-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeclineAsync_Throws_KeyNotFound_For_Missing_Request()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.DeclineAsync(Guid.NewGuid(), "seller-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
