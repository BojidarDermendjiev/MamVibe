using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

using MomVibe.Application.DTOs.Offers;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for OfferService using EF Core InMemory.
/// IOfferNotifier is a Moq mock — notifications are verified as best-effort fire-and-forget.
/// Users and Items are always seeded before Offers to avoid the InMemory Include+navigation bug.
/// </summary>
public class OfferServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"OfferTest_{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static OfferService CreateService(ApplicationDbContext db, Mock<IOfferNotifier>? notifierMock = null)
    {
        notifierMock ??= new Mock<IOfferNotifier>();
        return new OfferService(db, CreateMapper(), notifierMock.Object);
    }

    private static async Task<Item> SeedItemAsync(
        ApplicationDbContext db,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        ListingType listingType = ListingType.Sell,
        bool isActive = true,
        decimal price = 100m)
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser { Id = sellerId, DisplayName = "Seller", Email = $"{sellerId}@t.com", UserName = sellerId });

        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser { Id = buyerId, DisplayName = "Buyer", Email = $"{buyerId}@t.com", UserName = buyerId });

        var item = new Item
        {
            Title = "Baby Stroller",
            Description = "Good condition",
            UserId = sellerId,
            IsActive = isActive,
            ListingType = listingType,
            Price = listingType == ListingType.Donate ? null : price,
            Photos = [new ItemPhoto { Url = "https://example.com/photo.jpg", DisplayOrder = 0 }]
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private static async Task<Offer> SeedOfferAsync(
        ApplicationDbContext db,
        Guid itemId,
        string sellerId = "seller-1",
        string buyerId = "buyer-1",
        decimal offeredPrice = 75m,
        OfferStatus status = OfferStatus.Pending)
    {
        var offer = new Offer
        {
            ItemId = itemId,
            BuyerId = buyerId,
            SellerId = sellerId,
            OfferedPrice = offeredPrice,
            Status = status,
        };
        db.Offers.Add(offer);
        await db.SaveChangesAsync();
        return offer;
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Creates_Offer_And_Notifies_Seller()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var notifier = new Mock<IOfferNotifier>();
        notifier.Setup(n => n.NotifySellerAsync(It.IsAny<string>(), It.IsAny<OfferDto>()))
            .Returns(Task.CompletedTask);

        var svc = CreateService(db, notifier);
        var dto = new CreateOfferDto { ItemId = item.Id, OfferedPrice = 70m };

        var result = await svc.CreateAsync(dto, "buyer-1");

        result.Status.Should().Be(OfferStatus.Pending);
        result.OfferedPrice.Should().Be(70m);
        result.BuyerDisplayName.Should().Be("Buyer");
        result.SellerDisplayName.Should().Be("Seller");
        notifier.Verify(n => n.NotifySellerAsync("seller-1", It.IsAny<OfferDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_KeyNotFound_When_Item_Does_Not_Exist()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.CreateAsync(new CreateOfferDto { ItemId = Guid.NewGuid(), OfferedPrice = 50m }, "buyer-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_Throws_InvalidOperation_For_Donate_Item()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, listingType: ListingType.Donate);
        var svc = CreateService(db);

        var act = async () => await svc.CreateAsync(new CreateOfferDto { ItemId = item.Id, OfferedPrice = 0m }, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*sale*");
    }

    [Fact]
    public async Task CreateAsync_Throws_InvalidOperation_When_Seller_Makes_Offer_On_Own_Item()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, sellerId: "seller-1");
        var svc = CreateService(db);

        var act = async () => await svc.CreateAsync(new CreateOfferDto { ItemId = item.Id, OfferedPrice = 50m }, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*own item*");
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Buyer_Already_Has_Active_Offer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        await SeedOfferAsync(db, item.Id, status: OfferStatus.Pending);
        var svc = CreateService(db);

        var act = async () => await svc.CreateAsync(new CreateOfferDto { ItemId = item.Id, OfferedPrice = 60m }, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active offer*");
    }

    [Fact]
    public async Task CreateAsync_Allows_New_Offer_After_Previous_Was_Declined()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        await SeedOfferAsync(db, item.Id, status: OfferStatus.Declined);
        var svc = CreateService(db);

        var dto = new CreateOfferDto { ItemId = item.Id, OfferedPrice = 60m };
        var result = await svc.CreateAsync(dto, "buyer-1");

        result.Status.Should().Be(OfferStatus.Pending);
    }

    // =========================================================================
    // AcceptAsync
    // =========================================================================

    [Fact]
    public async Task AcceptAsync_Sets_Status_To_Accepted_And_Notifies_Buyer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id);
        var notifier = new Mock<IOfferNotifier>();

        var svc = CreateService(db, notifier);
        var result = await svc.AcceptAsync(offer.Id, "seller-1");

        result.Status.Should().Be(OfferStatus.Accepted);
        notifier.Verify(n => n.NotifyBuyerAsync("buyer-1", It.IsAny<OfferDto>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_Throws_Unauthorized_When_Not_Seller()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id);
        var svc = CreateService(db);

        var act = async () => await svc.AcceptAsync(offer.Id, "buyer-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task AcceptAsync_Throws_InvalidOperation_When_Offer_Not_Pending()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Declined);
        var svc = CreateService(db);

        var act = async () => await svc.AcceptAsync(offer.Id, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptAsync_Throws_KeyNotFound_For_Missing_Offer()
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
    public async Task DeclineAsync_Sets_Status_To_Declined_And_Notifies_Buyer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id);
        var notifier = new Mock<IOfferNotifier>();

        var svc = CreateService(db, notifier);
        var result = await svc.DeclineAsync(offer.Id, "seller-1");

        result.Status.Should().Be(OfferStatus.Declined);
        notifier.Verify(n => n.NotifyBuyerAsync("buyer-1", It.IsAny<OfferDto>()), Times.Once);
    }

    [Fact]
    public async Task DeclineAsync_Throws_Unauthorized_When_Not_Seller()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id);
        var svc = CreateService(db);

        var act = async () => await svc.DeclineAsync(offer.Id, "buyer-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeclineAsync_Throws_When_Offer_Not_Pending()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Accepted);
        var svc = CreateService(db);

        var act = async () => await svc.DeclineAsync(offer.Id, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // CounterAsync
    // =========================================================================

    [Fact]
    public async Task CounterAsync_Sets_Status_To_Countered_And_Stores_Counter_Price()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, offeredPrice: 70m);
        var notifier = new Mock<IOfferNotifier>();

        var svc = CreateService(db, notifier);
        var result = await svc.CounterAsync(offer.Id, "seller-1", 85m);

        result.Status.Should().Be(OfferStatus.Countered);
        result.CounterPrice.Should().Be(85m);
        notifier.Verify(n => n.NotifyBuyerAsync("buyer-1", It.IsAny<OfferDto>()), Times.Once);
    }

    [Fact]
    public async Task CounterAsync_Throws_When_Offer_Not_Pending()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Accepted);
        var svc = CreateService(db);

        var act = async () => await svc.CounterAsync(offer.Id, "seller-1", 85m);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // AcceptCounterAsync
    // =========================================================================

    [Fact]
    public async Task AcceptCounterAsync_Sets_Status_To_Accepted_And_Notifies_Seller()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Countered);
        var notifier = new Mock<IOfferNotifier>();

        var svc = CreateService(db, notifier);
        var result = await svc.AcceptCounterAsync(offer.Id, "buyer-1");

        result.Status.Should().Be(OfferStatus.Accepted);
        notifier.Verify(n => n.NotifySellerAsync("seller-1", It.IsAny<OfferDto>()), Times.Once);
    }

    [Fact]
    public async Task AcceptCounterAsync_Throws_When_Offer_Not_Countered()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Pending);
        var svc = CreateService(db);

        var act = async () => await svc.AcceptCounterAsync(offer.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptCounterAsync_Throws_Unauthorized_When_Not_Buyer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Countered);
        var svc = CreateService(db);

        var act = async () => await svc.AcceptCounterAsync(offer.Id, "seller-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =========================================================================
    // DeclineCounterAsync
    // =========================================================================

    [Fact]
    public async Task DeclineCounterAsync_Sets_Status_To_Declined()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Countered);
        var svc = CreateService(db);

        var result = await svc.DeclineCounterAsync(offer.Id, "buyer-1");

        result.Status.Should().Be(OfferStatus.Declined);
    }

    [Fact]
    public async Task DeclineCounterAsync_Throws_When_Offer_Not_Countered()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Pending);
        var svc = CreateService(db);

        var act = async () => await svc.DeclineCounterAsync(offer.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // CancelAsync
    // =========================================================================

    [Fact]
    public async Task CancelAsync_Sets_Status_To_Cancelled_For_Pending_Offer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Pending);
        var svc = CreateService(db);

        var result = await svc.CancelAsync(offer.Id, "buyer-1");

        result.Status.Should().Be(OfferStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_Sets_Status_To_Cancelled_For_Countered_Offer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Countered);
        var svc = CreateService(db);

        var result = await svc.CancelAsync(offer.Id, "buyer-1");

        result.Status.Should().Be(OfferStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_Throws_When_Offer_Already_Accepted()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Accepted);
        var svc = CreateService(db);

        var act = async () => await svc.CancelAsync(offer.Id, "buyer-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelAsync_Throws_Unauthorized_When_Not_Buyer()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db);
        var offer = await SeedOfferAsync(db, item.Id, status: OfferStatus.Pending);
        var svc = CreateService(db);

        var act = async () => await svc.CancelAsync(offer.Id, "seller-1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =========================================================================
    // GetReceivedAsync / GetSentAsync
    // =========================================================================

    [Fact]
    public async Task GetReceivedAsync_Returns_Only_Seller_Offers()
    {
        await using var db = CreateDb();
        var item1 = await SeedItemAsync(db, sellerId: "seller-1", buyerId: "buyer-1");
        var item2 = await SeedItemAsync(db, sellerId: "seller-2", buyerId: "buyer-2");
        await SeedOfferAsync(db, item1.Id, sellerId: "seller-1", buyerId: "buyer-1");
        await SeedOfferAsync(db, item2.Id, sellerId: "seller-2", buyerId: "buyer-2");

        var svc = CreateService(db);
        var result = await svc.GetReceivedAsync("seller-1");

        result.Should().HaveCount(1);
        result[0].SellerDisplayName.Should().Be("Seller");
    }

    [Fact]
    public async Task GetSentAsync_Returns_Only_Buyer_Offers()
    {
        await using var db = CreateDb();
        var item1 = await SeedItemAsync(db, sellerId: "seller-1", buyerId: "buyer-1");
        var item2 = await SeedItemAsync(db, sellerId: "seller-2", buyerId: "buyer-2");
        await SeedOfferAsync(db, item1.Id, sellerId: "seller-1", buyerId: "buyer-1");
        await SeedOfferAsync(db, item2.Id, sellerId: "seller-2", buyerId: "buyer-2");

        var svc = CreateService(db);
        var result = await svc.GetSentAsync("buyer-2");

        result.Should().HaveCount(1);
        result[0].BuyerDisplayName.Should().Be("Buyer");
    }

    [Fact]
    public async Task GetReceivedAsync_Returns_Empty_When_No_Offers()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetReceivedAsync("unknown-seller");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSentAsync_Returns_Empty_When_No_Offers()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetSentAsync("unknown-buyer");
        result.Should().BeEmpty();
    }
}
