using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using MomVibe.Application.DTOs.Bundles;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

public class BundleServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"BundleTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static BundleService CreateService(ApplicationDbContext db)
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Stripe:SecretKey"]).Returns(string.Empty);
        var shippingMock = new Mock<IShippingService>();
        return new BundleService(db, CreateMapper(), configMock.Object, shippingMock.Object, NullLogger<BundleService>.Instance);
    }

    private static async Task<ApplicationUser> SeedUserAsync(ApplicationDbContext db, string userId, string displayName = "Seller")
    {
        var user = new ApplicationUser
        {
            Id = userId,
            DisplayName = displayName,
            Email = $"{userId}@test.com",
            UserName = userId
        };
        if (!db.Users.Any(u => u.Id == userId))
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        return user;
    }

    private static async Task<Item> SeedItemAsync(ApplicationDbContext db, string userId, bool isActive = true, bool isReserved = false)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Test Item",
            Description = "Test description",
            UserId = userId,
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Sell,
            Condition = ItemCondition.Good,
            Price = 10m,
            IsActive = isActive,
            IsReserved = isReserved
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private static List<Guid> Ids(params Item[] items) => items.Select(i => i.Id).ToList();

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Persists_Bundle_And_Returns_Dto()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");

        var svc = CreateService(db);
        var dto = await svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "My Bundle",
            Description = "A nice bundle",
            Price = 30m,
            ItemIds = Ids(item1, item2)
        });

        dto.Title.Should().Be("My Bundle");
        dto.Price.Should().Be(30m);
        dto.Items.Should().HaveCount(2);
        dto.SellerId.Should().Be("seller-1");
        (await db.Bundles.CountAsync()).Should().Be(1);
        (await db.BundleItems.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_ThrowsKeyNotFound_WhenItemNotInDb()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "Bundle",
            Price = 20m,
            ItemIds = [item1.Id, Guid.NewGuid()] // second ID doesn't exist
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperation_WhenItemBelongsToDifferentSeller()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        await SeedUserAsync(db, "seller-2");
        var ownItem = await SeedItemAsync(db, "seller-1");
        var otherItem = await SeedItemAsync(db, "seller-2");

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "Bundle",
            Price = 20m,
            ItemIds = Ids(ownItem, otherItem)
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*own active*");
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperation_WhenItemIsInActiveBundle()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");

        // Pre-existing active bundle that includes item1
        var existing = new Bundle
        {
            Title = "Old Bundle",
            Price = 20m,
            SellerId = "seller-1",
            IsActive = true,
            IsSold = false,
            BundleItems = [new BundleItem { ItemId = item1.Id }]
        };
        db.Bundles.Add(existing);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "New Bundle",
            Price = 25m,
            ItemIds = Ids(item1, item2)
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already part of an active bundle*");
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperation_WhenFewerThan2Items()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "Bundle",
            Price = 10m,
            ItemIds = [item1.Id]
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*between 2 and 10*");
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperation_WhenMoreThan10Items()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var itemIds = new List<Guid>();
        for (int i = 0; i < 11; i++)
        {
            var item = await SeedItemAsync(db, "seller-1");
            itemIds.Add(item.Id);
        }

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("seller-1", new CreateBundleDto
        {
            Title = "Bundle",
            Price = 100m,
            ItemIds = itemIds
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*between 2 and 10*");
    }

    // =========================================================================
    // GetMyAsync
    // =========================================================================

    [Fact]
    public async Task GetMyAsync_Returns_Only_Seller_Bundles()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        await SeedUserAsync(db, "seller-2");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");
        var item3 = await SeedItemAsync(db, "seller-2");
        var item4 = await SeedItemAsync(db, "seller-2");

        db.Bundles.Add(new Bundle
        {
            Title = "Bundle A",
            Price = 20m,
            SellerId = "seller-1",
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        });
        db.Bundles.Add(new Bundle
        {
            Title = "Bundle B",
            Price = 30m,
            SellerId = "seller-2",
            BundleItems = [new BundleItem { ItemId = item3.Id }, new BundleItem { ItemId = item4.Id }]
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetMyAsync("seller-1");

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Bundle A");
    }

    [Fact]
    public async Task GetMyAsync_Returns_Empty_When_No_Bundles()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");

        var svc = CreateService(db);
        var result = await svc.GetMyAsync("seller-1");

        result.Should().BeEmpty();
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_Bundle_From_Db()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");

        var bundle = new Bundle
        {
            Title = "Bundle",
            Price = 20m,
            SellerId = "seller-1",
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        await svc.DeleteAsync(bundle.Id, "seller-1");

        (await db.Bundles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsKeyNotFound_WhenBundleDoesNotExist()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");

        var svc = CreateService(db);
        var act = () => svc.DeleteAsync(Guid.NewGuid(), "seller-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsUnauthorized_WhenCallerIsNotSeller()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        await SeedUserAsync(db, "other-user");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");

        var bundle = new Bundle
        {
            Title = "Bundle",
            Price = 20m,
            SellerId = "seller-1",
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.DeleteAsync(bundle.Id, "other-user");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperation_WhenBundleIsSold()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-1");
        var item1 = await SeedItemAsync(db, "seller-1");
        var item2 = await SeedItemAsync(db, "seller-1");

        var bundle = new Bundle
        {
            Title = "Bundle",
            Price = 20m,
            SellerId = "seller-1",
            IsSold = true,
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.DeleteAsync(bundle.Id, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*sold*");
    }

    // =========================================================================
    // Checkout / payment completion — bundle sold state
    // =========================================================================

    [Fact]
    public async Task CreateCheckoutSessionAsync_TestMode_MarksAllBundleItemsSold()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-pay");
        var item1 = await SeedItemAsync(db, "seller-pay");
        var item2 = await SeedItemAsync(db, "seller-pay");

        var bundle = new Bundle
        {
            Title = "Pay Bundle", Price = 30m, SellerId = "seller-pay", IsActive = true,
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        var url = await svc.CreateCheckoutSessionAsync(
            bundle.Id, buyerId: "buyer-pay",
            successUrl: "https://app/success",
            cancelUrl: "https://app/cancel");

        url.Should().Contain("success", because: "test-mode returns the success URL immediately");

        var updated1 = await db.Items.FindAsync(item1.Id);
        var updated2 = await db.Items.FindAsync(item2.Id);
        updated1!.IsSold.Should().BeTrue(because: "every item in the bundle must be marked sold after payment");
        updated1.IsActive.Should().BeFalse();
        updated2!.IsSold.Should().BeTrue();
        updated2.IsActive.Should().BeFalse();

        var updatedBundle = await db.Bundles.FindAsync(bundle.Id);
        updatedBundle!.IsSold.Should().BeTrue(because: "the bundle itself must be marked sold");
        updatedBundle.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_TestMode_CreatesPaymentRecord()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-pr");
        var item1 = await SeedItemAsync(db, "seller-pr");
        var item2 = await SeedItemAsync(db, "seller-pr");

        var bundle = new Bundle
        {
            Title = "PR Bundle", Price = 50m, SellerId = "seller-pr", IsActive = true,
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        await svc.CreateCheckoutSessionAsync(bundle.Id, "buyer-pr", "https://app/s", "https://app/c");

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.BundleId == bundle.Id);
        payment.Should().NotBeNull();
        payment!.BuyerId.Should().Be("buyer-pr");
        payment.SellerId.Should().Be("seller-pr");
        payment.Amount.Should().Be(50m);
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ThrowsIfSellerBuysOwnBundle()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "seller-self");
        var item1 = await SeedItemAsync(db, "seller-self");
        var item2 = await SeedItemAsync(db, "seller-self");

        var bundle = new Bundle
        {
            Title = "Self Bundle", Price = 20m, SellerId = "seller-self", IsActive = true,
            BundleItems = [new BundleItem { ItemId = item1.Id }, new BundleItem { ItemId = item2.Id }]
        };
        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.CreateCheckoutSessionAsync(bundle.Id, "seller-self", "https://app/s", "https://app/c");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot purchase your own bundle*");
    }
}
