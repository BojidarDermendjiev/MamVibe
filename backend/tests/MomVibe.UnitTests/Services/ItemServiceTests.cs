using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.DTOs.Items;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for ItemService using EF Core InMemory.
/// AI moderation, webhook, and nekorekten dependencies are Moq mocks.
/// Follows the known bug pattern: always seed ApplicationUser before Items that reference it.
/// TransactionIgnoredWarning is suppressed because InMemory does not support real transactions
/// but the service code uses BeginTransactionAsync inside ToggleLikeAsync.
/// </summary>
public class ItemServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ItemTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static ItemService CreateService(
        ApplicationDbContext db,
        Mock<IAiService>? aiMock = null,
        Mock<INekorektenService>? nekorektenMock = null,
        IPriceDropNotifier? priceDropNotifier = null)
    {
        aiMock ??= new Mock<IAiService>();
        nekorektenMock ??= new Mock<INekorektenService>();
        var webhookMock = new Mock<IN8nWebhookService>();
        var n8nOptions = Options.Create(new N8nSettings());

        var cacheMock = new Mock<IMemoryCache>();
        var cacheEntry = new Mock<ICacheEntry>();
        cacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny)).Returns(false);
        cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);

        return new ItemService(
            db,
            CreateMapper(),
            webhookMock.Object,
            n8nOptions,
            aiMock.Object,
            nekorektenMock.Object,
            cacheMock.Object,
            priceDropNotifier: priceDropNotifier);
    }

    /// <summary>
    /// Seeds a user and an active item; returns the seeded item.
    /// The user is always seeded first to avoid the InMemory Include(User) bug.
    /// </summary>
    private static async Task<Item> SeedItemAsync(
        ApplicationDbContext db,
        string userId = "seller-1",
        bool isActive = true,
        ListingType listingType = ListingType.Sell,
        decimal? price = 50m,
        Guid? categoryId = null,
        AgeGroup? ageGroup = null)
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = "Seller",
                Email = $"{userId}@test.com",
                UserName = userId
            });

        var catId = categoryId ?? Guid.NewGuid();
        if (!db.Categories.Any(c => c.Id == catId))
            db.Categories.Add(new Category { Id = catId, Name = "Clothing", Slug = $"clothing-{catId}" });

        var item = new Item
        {
            Title = "Test Item",
            Description = "A description for the test item",
            UserId = userId,
            IsActive = isActive,
            ListingType = listingType,
            Price = price,
            CategoryId = catId,
            AgeGroup = ageGroup,
            Photos = [new ItemPhoto { Url = "https://example.com/photo.jpg", DisplayOrder = 0 }]
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_Returns_Only_Active_Items()
    {
        await using var db = CreateDb();
        await SeedItemAsync(db, userId: "u1", isActive: true);
        await SeedItemAsync(db, userId: "u2", isActive: false);

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(new ItemFilterDto { Page = 1, PageSize = 12 });

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_Filters_By_ListingType()
    {
        await using var db = CreateDb();
        await SeedItemAsync(db, userId: "u1", listingType: ListingType.Sell);
        await SeedItemAsync(db, userId: "u2", listingType: ListingType.Donate);

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(new ItemFilterDto
        {
            ListingType = ListingType.Sell,
            Page = 1,
            PageSize = 12
        });

        result.Items.Should().HaveCount(1);
        result.Items[0].ListingType.Should().Be(ListingType.Sell);
    }

    [Fact]
    public async Task GetAllAsync_SearchTerm_Filters_By_Title_And_Description()
    {
        await using var db = CreateDb();

        // Seed user first
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "U1", Email = "u1@test.com", UserName = "u1" });
        var catId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = catId, Name = "Cat", Slug = $"cat-{catId}" });
        db.Items.Add(new Item { Title = "Blue Sneakers", Description = "Great shoes", UserId = "u1", IsActive = true, CategoryId = catId });
        db.Items.Add(new Item { Title = "Red Dress",    Description = "Beautiful dress", UserId = "u1", IsActive = true, CategoryId = catId });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(new ItemFilterDto
        {
            SearchTerm = "sneakers",
            Page = 1,
            PageSize = 12
        });

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Blue Sneakers");
    }

    [Fact]
    public async Task GetAllAsync_Marks_IsLikedByCurrentUser_Correctly()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");
        db.Likes.Add(new Like { UserId = "liker-1", ItemId = item.Id });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(new ItemFilterDto { Page = 1, PageSize = 12 }, currentUserId: "liker-1");

        result.Items.Should().HaveCount(1);
        result.Items[0].IsLikedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_Paginates_Results()
    {
        await using var db = CreateDb();
        for (var i = 0; i < 5; i++)
            await SeedItemAsync(db, userId: $"u{i}");

        var svc = CreateService(db);
        var page1 = await svc.GetAllAsync(new ItemFilterDto { Page = 1, PageSize = 3 });
        var page2 = await svc.GetAllAsync(new ItemFilterDto { Page = 2, PageSize = 3 });

        page1.Items.Should().HaveCount(3);
        page2.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Item_Not_Found()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Item_With_Correct_Data()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 99.99m);

        var svc = CreateService(db);
        var result = await svc.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Price.Should().Be(99.99m);
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Saves_Item_And_Photos_To_Database()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "creator", DisplayName = "Creator", Email = "c@test.com", UserName = "creator" });
        var catId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = catId, Name = "Toys", Slug = $"toys-{catId}" });
        await db.SaveChangesAsync();

        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ModerateItemAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ListingType>(), It.IsAny<decimal?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AiModerationResultDto
            {
                Recommendation = "approve",
                Confidence = 0.97,
                Reason = "Appropriate content"
            });

        var svc = CreateService(db, aiMock);
        var dto = new CreateItemDto
        {
            Title = "Toy Car",
            Description = "A nice red toy car",
            CategoryId = catId,
            ListingType = ListingType.Sell,
            Price = 15m,
            PhotoUrls = ["https://example.com/car.jpg", "https://example.com/car2.jpg"]
        };

        var result = await svc.CreateAsync(dto, "creator");

        result.Should().NotBeNull();
        result.Title.Should().Be("Toy Car");

        var savedItem = await db.Items.Include(i => i.Photos).FirstAsync();
        savedItem.Photos.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_AutoApproves_Item_When_AI_Score_Exceeds_Threshold_For_Trusted_Seller()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "trusted", DisplayName = "Trusted", Email = "t@test.com", UserName = "trusted" });
        var catId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = catId, Name = "Gear", Slug = $"gear-{catId}" });

        // Seed 3+ completed payments to make this seller "trusted"
        var itemId = Guid.NewGuid();
        db.Items.Add(new Item { Id = itemId, Title = "Old", Description = "Old desc", UserId = "trusted", CategoryId = catId });
        for (var i = 0; i < 3; i++)
        {
            db.Payments.Add(new Payment
            {
                SellerId = "trusted",
                BuyerId = $"buyer-{i}",
                ItemId = itemId,
                Amount = 10m,
                Status = PaymentStatus.Completed,
                PaymentMethod = PaymentMethod.Card
            });
        }
        await db.SaveChangesAsync();

        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ModerateItemAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ListingType>(), It.IsAny<decimal?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AiModerationResultDto
            {
                Recommendation = "approve",
                Confidence = 0.90,  // >= 0.85 threshold for trusted sellers
                Reason = "Looks good"
            });

        var svc = CreateService(db, aiMock);
        var dto = new CreateItemDto
        {
            Title = "Baby Jacket",
            Description = "Warm winter jacket",
            CategoryId = catId,
            ListingType = ListingType.Sell,
            Price = 40m,
            PhotoUrls = ["https://example.com/jacket.jpg"]
        };

        await svc.CreateAsync(dto, "trusted");

        var saved = await db.Items.Where(i => i.Title == "Baby Jacket").FirstAsync();
        saved.IsActive.Should().BeTrue();
        saved.AiModerationStatus.Should().Be(AiModerationStatus.AutoApproved);
    }

    [Fact]
    public async Task CreateAsync_Does_Not_Throw_When_AI_Moderation_Fails()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "U1", Email = "u1@test.com", UserName = "u1" });
        var catId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = catId, Name = "Cat", Slug = $"cat-{catId}" });
        await db.SaveChangesAsync();

        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.ModerateItemAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ListingType>(), It.IsAny<decimal?>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("AI service down"));

        var svc = CreateService(db, aiMock);
        var act = async () => await svc.CreateAsync(new CreateItemDto
        {
            Title = "Resilient Item",
            Description = "Created even when AI fails",
            CategoryId = catId,
            ListingType = ListingType.Donate,
            PhotoUrls = []
        }, "u1");

        // Should not propagate AI failure
        await act.Should().NotThrowAsync();
    }

    // =========================================================================
    // UpdateAsync
    // =========================================================================

    [Fact]
    public async Task UpdateAsync_Updates_Title_And_Description()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");

        var svc = CreateService(db);
        var result = await svc.UpdateAsync(item.Id, new UpdateItemDto
        {
            Title = "Updated Title",
            Description = "Updated description for the item"
        }, "owner-1");

        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateAsync_Throws_KeyNotFound_When_Item_Does_Not_Exist()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.UpdateAsync(Guid.NewGuid(), new UpdateItemDto
        {
            Title = "New Title"
        }, "user-1");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found.");
    }

    [Fact]
    public async Task UpdateAsync_Throws_Unauthorized_When_Not_Owner()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");
        var svc = CreateService(db);

        var act = async () => await svc.UpdateAsync(item.Id, new UpdateItemDto
        {
            Title = "Hijacked Title"
        }, "other-user");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*edit your own items*");
    }

    [Fact]
    public async Task UpdateAsync_Replaces_Photos_When_PhotoUrls_Provided()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");

        var svc = CreateService(db);
        await svc.UpdateAsync(item.Id, new UpdateItemDto
        {
            PhotoUrls = ["https://example.com/new1.jpg", "https://example.com/new2.jpg"]
        }, "owner-1");

        var photos = await db.ItemPhotos.Where(p => p.ItemId == item.Id).ToListAsync();
        photos.Should().HaveCount(2);
        photos.Should().AllSatisfy(p => p.Url.Should().StartWith("https://example.com/new"));
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_Item_From_Database()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");
        var svc = CreateService(db);

        await svc.DeleteAsync(item.Id, "owner-1");

        var deleted = await db.Items.FindAsync(item.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_KeyNotFound_When_Item_Missing()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.DeleteAsync(Guid.NewGuid(), "user-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Throws_Unauthorized_When_Not_Owner_And_Not_Admin()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");
        var svc = CreateService(db);

        var act = async () => await svc.DeleteAsync(item.Id, "other-user", isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_For_Admin_Regardless_Of_Ownership()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "owner-1");
        var svc = CreateService(db);

        await svc.DeleteAsync(item.Id, "admin-user", isAdmin: true);

        var deleted = await db.Items.FindAsync(item.Id);
        deleted.Should().BeNull();
    }

    // =========================================================================
    // IncrementViewCountAsync
    // =========================================================================

    [Fact]
    public async Task IncrementViewCountAsync_Increments_Counter_By_One()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");
        var initialCount = item.ViewCount;

        var svc = CreateService(db);
        await svc.IncrementViewCountAsync(item.Id);

        var updated = await db.Items.FindAsync(item.Id);
        updated!.ViewCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_Does_Not_Throw_For_Missing_Item()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.IncrementViewCountAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    // =========================================================================
    // ToggleLikeAsync
    // =========================================================================

    [Fact]
    public async Task ToggleLikeAsync_Returns_True_And_Increments_LikeCount_When_Liking()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");

        var svc = CreateService(db);
        var isNowLiked = await svc.ToggleLikeAsync(item.Id, "liker-1");

        isNowLiked.Should().BeTrue();
        var updated = await db.Items.FindAsync(item.Id);
        updated!.LikeCount.Should().Be(1);
        var like = await db.Likes.FirstOrDefaultAsync(l => l.ItemId == item.Id && l.UserId == "liker-1");
        like.Should().NotBeNull();
    }

    [Fact]
    public async Task ToggleLikeAsync_Returns_False_And_Decrements_LikeCount_When_Unliking()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");
        item.LikeCount = 1;
        db.Likes.Add(new Like { UserId = "liker-1", ItemId = item.Id });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var isNowLiked = await svc.ToggleLikeAsync(item.Id, "liker-1");

        isNowLiked.Should().BeFalse();
        var updated = await db.Items.FindAsync(item.Id);
        updated!.LikeCount.Should().Be(0);
        var like = await db.Likes.FirstOrDefaultAsync(l => l.ItemId == item.Id && l.UserId == "liker-1");
        like.Should().BeNull();
    }

    [Fact]
    public async Task ToggleLikeAsync_Throws_KeyNotFound_When_Item_Missing()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.ToggleLikeAsync(Guid.NewGuid(), "user-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // GetUserItemsAsync
    // =========================================================================

    [Fact]
    public async Task GetUserItemsAsync_Returns_Only_Items_Belonging_To_User()
    {
        await using var db = CreateDb();
        await SeedItemAsync(db, userId: "owner-1");
        await SeedItemAsync(db, userId: "owner-1");
        await SeedItemAsync(db, userId: "other-user");

        var svc = CreateService(db);
        var result = await svc.GetUserItemsAsync("owner-1");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(i => i.UserId.Should().Be("owner-1"));
    }

    [Fact]
    public async Task GetUserItemsAsync_Returns_Empty_When_User_Has_No_Items()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetUserItemsAsync("nobody");
        result.Should().BeEmpty();
    }

    // =========================================================================
    // GetLikedItemsAsync
    // =========================================================================

    [Fact]
    public async Task GetLikedItemsAsync_Returns_Items_User_Has_Liked()
    {
        await using var db = CreateDb();
        var item1 = await SeedItemAsync(db, userId: "seller-1");
        var item2 = await SeedItemAsync(db, userId: "seller-2");

        db.Likes.Add(new Like { UserId = "fan", ItemId = item1.Id });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetLikedItemsAsync("fan");

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(item1.Id);
        result[0].IsLikedByCurrentUser.Should().BeTrue();
        _ = item2; // not liked
    }

    // =========================================================================
    // BumpAsync
    // =========================================================================

    [Fact]
    public async Task BumpAsync_Sets_BumpedAt_And_Returns_Dto()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");

        var svc = CreateService(db);
        var before = DateTime.UtcNow;
        var result = await svc.BumpAsync(item.Id, "seller-1");
        var after = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.BumpedAt.Should().NotBeNull();
        result.BumpedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task BumpAsync_Throws_KeyNotFoundException_When_Item_Not_Found()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = () => svc.BumpAsync(Guid.NewGuid(), "seller-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task BumpAsync_Throws_UnauthorizedAccessException_When_Not_Owner()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");

        var svc = CreateService(db);
        var act = () => svc.BumpAsync(item.Id, "other-user");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task BumpAsync_Throws_InvalidOperationException_When_On_Cooldown()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");
        item.BumpedAt = DateTime.UtcNow.AddDays(-1); // bumped 1 day ago — cooldown is 7 days
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.BumpAsync(item.Id, "seller-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bump*");
    }

    [Fact]
    public async Task BumpAsync_Allows_Bump_After_Cooldown_Expires()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1");
        item.BumpedAt = DateTime.UtcNow.AddDays(-8); // bumped 8 days ago — past the 7-day cooldown
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.BumpAsync(item.Id, "seller-1");

        result.BumpedAt.Should().NotBeNull();
        result.BumpedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // =========================================================================
    // UpdateAsync — Price Drop Notifications
    // =========================================================================

    [Fact]
    public async Task UpdateAsync_Notifies_All_Likers_When_Price_Drops()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 50m);

        // Two likers
        db.Users.Add(new ApplicationUser { Id = "liker-a", DisplayName = "A", Email = "a@test.com", UserName = "liker-a" });
        db.Users.Add(new ApplicationUser { Id = "liker-b", DisplayName = "B", Email = "b@test.com", UserName = "liker-b" });
        db.Likes.Add(new Like { UserId = "liker-a", ItemId = item.Id });
        db.Likes.Add(new Like { UserId = "liker-b", ItemId = item.Id });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<IPriceDropNotifier>();
        notifierMock.Setup(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<PriceDropNotification>()))
            .Returns(Task.CompletedTask);

        var svc = CreateService(db, priceDropNotifier: notifierMock.Object);
        await svc.UpdateAsync(item.Id, new UpdateItemDto { Price = 30m }, "seller-1");

        notifierMock.Verify(n => n.NotifyAsync("liker-a", It.Is<PriceDropNotification>(p =>
            p.OldPrice == 50m && p.NewPrice == 30m && p.ItemId == item.Id)), Times.Once);
        notifierMock.Verify(n => n.NotifyAsync("liker-b", It.Is<PriceDropNotification>(p =>
            p.OldPrice == 50m && p.NewPrice == 30m)), Times.Once);
        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<PriceDropNotification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Notify_When_Price_Does_Not_Change()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 40m);

        db.Users.Add(new ApplicationUser { Id = "liker-a", DisplayName = "A", Email = "a@test.com", UserName = "liker-a" });
        db.Likes.Add(new Like { UserId = "liker-a", ItemId = item.Id });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<IPriceDropNotifier>();
        var svc = CreateService(db, priceDropNotifier: notifierMock.Object);

        await svc.UpdateAsync(item.Id, new UpdateItemDto { Price = 40m }, "seller-1");

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<PriceDropNotification>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Notify_When_Price_Increases()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 20m);

        db.Users.Add(new ApplicationUser { Id = "liker-a", DisplayName = "A", Email = "a@test.com", UserName = "liker-a" });
        db.Likes.Add(new Like { UserId = "liker-a", ItemId = item.Id });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<IPriceDropNotifier>();
        var svc = CreateService(db, priceDropNotifier: notifierMock.Object);

        await svc.UpdateAsync(item.Id, new UpdateItemDto { Price = 35m }, "seller-1");

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<PriceDropNotification>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Notify_When_Item_Has_No_Likers()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 60m);

        var notifierMock = new Mock<IPriceDropNotifier>();
        var svc = CreateService(db, priceDropNotifier: notifierMock.Object);

        await svc.UpdateAsync(item.Id, new UpdateItemDto { Price = 45m }, "seller-1");

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<PriceDropNotification>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_Without_PriceDropNotifier_Registered()
    {
        await using var db = CreateDb();
        var item = await SeedItemAsync(db, userId: "seller-1", price: 50m);

        db.Users.Add(new ApplicationUser { Id = "liker-a", DisplayName = "A", Email = "a@test.com", UserName = "liker-a" });
        db.Likes.Add(new Like { UserId = "liker-a", ItemId = item.Id });
        await db.SaveChangesAsync();

        // No notifier injected
        var svc = CreateService(db);
        var act = async () => await svc.UpdateAsync(item.Id, new UpdateItemDto { Price = 25m }, "seller-1");

        await act.Should().NotThrowAsync();
    }
}
