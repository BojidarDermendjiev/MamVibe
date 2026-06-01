using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
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
        return new PurchaseRequestService(
            db,
            CreateMapper(),
            notifierMock.Object,
            nekorektenMock.Object,
            new Mock<MediatR.IPublisher>().Object,
            NullLogger<PurchaseRequestService>.Instance);
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
        // Seed with isActive: true, isReserved: true to represent an item that has a pending request.
        // The new reservation model keeps IsActive = true so the item stays visible with a "Reserved" badge.
        var item = await SeedItemAsync(db, sellerId, buyerId, listingType, isActive: true);
        item.IsReserved = true;
        await db.SaveChangesAsync();

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
    public async Task AcceptAsync_Sets_Status_To_Accepted_For_Donate_Item_Without_Creating_Payment()
    {
        // New behavior: AcceptAsync only sets Status = Accepted for ALL item types.
        // The buyer fills in shipping details on the PaymentPage afterwards;
        // CreateBookingAsync (donate) or CreateCheckoutSessionAsync (sell) create the payment there.
        await using var db = CreateDb();
        var (_, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending, listingType: ListingType.Donate);

        var svc = CreateService(db);
        var result = await svc.AcceptAsync(request.Id, "seller-1");

        result.Status.Should().Be(PurchaseRequestStatus.Accepted);

        var updated = await db.PurchaseRequests.FindAsync(request.Id);
        updated!.Status.Should().Be(PurchaseRequestStatus.Accepted);

        // No payment record should have been created by AcceptAsync
        var payment = await db.Payments.FirstOrDefaultAsync(p =>
            p.BuyerId == "buyer-1" && p.SellerId == "seller-1");
        payment.Should().BeNull();
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
        // New behavior: decline sets item.IsReserved = false (IsActive is not touched).
        // The item was seeded with IsActive = true, IsReserved = true (pending request state).
        await using var db = CreateDb();
        var (item, request) = await SeedRequestAsync(db, sellerId: "seller-1", buyerId: "buyer-1",
            status: PurchaseRequestStatus.Pending);

        var svc = CreateService(db);
        var result = await svc.DeclineAsync(request.Id, "seller-1");

        result.Status.Should().Be(PurchaseRequestStatus.Declined);

        // IsReserved should be cleared; IsActive remains true (item stays visible)
        var restored = await db.Items.FindAsync(item.Id);
        restored!.IsReserved.Should().BeFalse();
        restored.IsActive.Should().BeTrue();
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

    [Fact]
    public async Task DeclineAsync_Bundle_Request_Unreserves_All_Bundle_Items()
    {
        await using var db = CreateDb();
        var bundle = await SeedBundleAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        // Reserve all bundle items (mirrors what CreateForBundleAsync does)
        var bundleItems = db.BundleItems.Include(bi => bi.Item).Where(bi => bi.BundleId == bundle.Id).ToList();
        foreach (var bi in bundleItems) bi.Item.IsReserved = true;

        var request = new PurchaseRequest
        {
            BundleId = bundle.Id,
            ItemId = null,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Status = PurchaseRequestStatus.Pending
        };
        db.PurchaseRequests.Add(request);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.DeclineAsync(request.Id, "seller-1");

        result.Status.Should().Be(PurchaseRequestStatus.Declined);

        var items = await db.BundleItems.Include(bi => bi.Item)
            .Where(bi => bi.BundleId == bundle.Id).ToListAsync();
        items.Should().AllSatisfy(bi => bi.Item.IsReserved.Should().BeFalse());
    }

    // =========================================================================
    // CreateForBundleAsync
    // =========================================================================

    [Fact]
    public async Task CreateForBundleAsync_Creates_Request_And_Reserves_All_Items()
    {
        await using var db = CreateDb();
        var bundle = await SeedBundleAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        var svc = CreateService(db);
        var result = await svc.CreateForBundleAsync(bundle.Id, "buyer-1");

        result.BundleId.Should().Be(bundle.Id);
        result.Status.Should().Be(PurchaseRequestStatus.Pending);

        var items = await db.BundleItems.Include(bi => bi.Item)
            .Where(bi => bi.BundleId == bundle.Id).ToListAsync();
        items.Should().AllSatisfy(bi => bi.Item.IsReserved.Should().BeTrue());
    }

    [Fact]
    public async Task CreateForBundleAsync_Throws_KeyNotFound_When_Bundle_Missing()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = () => svc.CreateForBundleAsync(Guid.NewGuid(), "buyer-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateForBundleAsync_Throws_InvalidOperation_When_Buyer_Is_Seller()
    {
        await using var db = CreateDb();
        var bundle = await SeedBundleAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        var svc = CreateService(db);
        var act = () => svc.CreateForBundleAsync(bundle.Id, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*own bundle*");
    }

    [Fact]
    public async Task CreateForBundleAsync_Throws_InvalidOperation_When_Bundle_Is_Sold()
    {
        await using var db = CreateDb();
        var bundle = await SeedBundleAsync(db, sellerId: "seller-1", buyerId: "buyer-1", isSold: true);

        var svc = CreateService(db);
        var act = () => svc.CreateForBundleAsync(bundle.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task CreateForBundleAsync_Throws_InvalidOperation_When_Active_Request_Exists()
    {
        await using var db = CreateDb();
        var bundle = await SeedBundleAsync(db, sellerId: "seller-1", buyerId: "buyer-1");

        // Seed an existing pending request for the same bundle+buyer
        db.PurchaseRequests.Add(new PurchaseRequest
        {
            BundleId = bundle.Id,
            ItemId = null,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Status = PurchaseRequestStatus.Pending
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.CreateForBundleAsync(bundle.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already have an active request*");
    }

    // =========================================================================
    // Bundle seed helper
    // =========================================================================

    private static async Task<Bundle> SeedBundleAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        int itemCount = 2,
        bool isActive = true,
        bool isSold = false)
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser { Id = sellerId, DisplayName = "Seller", Email = $"{sellerId}@test.com", UserName = sellerId });
        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser { Id = buyerId, DisplayName = "Buyer", Email = $"{buyerId}@test.com", UserName = buyerId });

        var items = Enumerable.Range(1, itemCount).Select(i => new Item
        {
            Title = $"Bundle Item {i}",
            Description = "Test item",
            UserId = sellerId,
            IsActive = true,
            ListingType = ListingType.Sell,
            Price = 20m
        }).ToList();
        db.Items.AddRange(items);
        await db.SaveChangesAsync();

        var bundle = new Bundle
        {
            Title = "Test Bundle",
            Price = 30m,
            SellerId = sellerId,
            IsActive = isActive,
            IsSold = isSold,
            BundleItems = items.Select(i => new BundleItem { ItemId = i.Id }).ToList()
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();
        return bundle;
    }
}
