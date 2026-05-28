using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

using MomVibe.Application.DTOs.Items;
using MomVibe.Application.DTOs.SavedSearches;
using MomVibe.Application.Interfaces;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

public class SavedSearchServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"SavedSearchTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static SavedSearchService CreateService(ApplicationDbContext db, Mock<ISavedSearchNotifier>? notifierMock = null)
    {
        notifierMock ??= new Mock<ISavedSearchNotifier>();
        notifierMock.Setup(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()))
                    .Returns(Task.CompletedTask);
        return new SavedSearchService(db, notifierMock.Object);
    }

    private static async Task SeedUserAsync(ApplicationDbContext db, string userId, string displayName = "User")
    {
        if (!db.Users.Any(u => u.Id == userId))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = displayName,
                Email = $"{userId}@test.com",
                UserName = userId
            });
            await db.SaveChangesAsync();
        }
    }

    private static ItemDto MakeItemDto(string userId = "seller-1", string title = "Baby Stroller", string description = "Great condition", decimal? price = 50m)
        => new ItemDto
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            UserId = userId,
            CategoryId = Guid.Empty,
            ListingType = ListingType.Sell,
            Condition = ItemCondition.Good,
            Price = price,
            IsActive = true
        };

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Persists_And_Returns_Dto()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "user-1");

        var svc = CreateService(db);
        var dto = await svc.CreateAsync("user-1", new CreateSavedSearchDto { Name = "My Search" });

        dto.Name.Should().Be("My Search");
        dto.Id.Should().NotBe(Guid.Empty);
        db.SavedSearches.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_Trims_Whitespace_From_SearchTerm()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "user-1");

        var svc = CreateService(db);
        var dto = await svc.CreateAsync("user-1", new CreateSavedSearchDto { Name = "Test", SearchTerm = "  stroller  " });

        dto.SearchTerm.Should().Be("stroller");
    }

    [Fact]
    public async Task CreateAsync_Throws_When_User_Has_20_Saved_Searches()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "user-1");

        for (var i = 0; i < 20; i++)
        {
            db.SavedSearches.Add(new SavedSearch { UserId = "user-1", Name = $"Search {i}" });
        }
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.CreateAsync("user-1", new CreateSavedSearchDto { Name = "One More" });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_SavedSearch()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "user-1");
        var entity = new SavedSearch { UserId = "user-1", Name = "To Delete" };
        db.SavedSearches.Add(entity);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        await svc.DeleteAsync(entity.Id, "user-1");

        db.SavedSearches.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_Throws_KeyNotFoundException_When_Not_Found()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = () => svc.DeleteAsync(Guid.NewGuid(), "user-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Throws_UnauthorizedAccessException_For_Wrong_User()
    {
        await using var db = CreateDb();
        var entity = new SavedSearch { UserId = "owner", Name = "Secret" };
        db.SavedSearches.Add(entity);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = () => svc.DeleteAsync(entity.Id, "other-user");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =========================================================================
    // GetMyAsync
    // =========================================================================

    [Fact]
    public async Task GetMyAsync_Returns_Only_Current_Users_Searches()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "user-1", Name = "Mine" });
        db.SavedSearches.Add(new SavedSearch { UserId = "user-2", Name = "Theirs" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetMyAsync("user-1");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Mine");
    }

    // =========================================================================
    // NotifyMatchingSearchesAsync
    // =========================================================================

    [Fact]
    public async Task NotifyMatchingSearchesAsync_Does_Not_Notify_Item_Owner()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "seller-1", Name = "Own search" });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<ISavedSearchNotifier>();
        var svc = CreateService(db, notifierMock);
        var item = MakeItemDto("seller-1");

        await svc.NotifyMatchingSearchesAsync(item);

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()), Times.Never);
    }

    [Fact]
    public async Task NotifyMatchingSearchesAsync_Notifies_Matching_Search()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "buyer-1", Name = "Strollers", SearchTerm = "Stroller" });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<ISavedSearchNotifier>();
        notifierMock.Setup(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()))
                    .Returns(Task.CompletedTask);

        var svc = CreateService(db, notifierMock);
        var item = MakeItemDto("seller-1", title: "Baby Stroller XL");

        await svc.NotifyMatchingSearchesAsync(item);

        notifierMock.Verify(n => n.NotifyAsync("buyer-1", It.IsAny<SavedSearchMatchNotification>()), Times.Once);
    }

    [Fact]
    public async Task NotifyMatchingSearchesAsync_Does_Not_Notify_When_SearchTerm_Does_Not_Match()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "buyer-1", Name = "Bicycles", SearchTerm = "bicycle" });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<ISavedSearchNotifier>();
        var svc = CreateService(db, notifierMock);
        var item = MakeItemDto("seller-1", title: "Baby Stroller", description: "Pink stroller");

        await svc.NotifyMatchingSearchesAsync(item);

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()), Times.Never);
    }

    [Fact]
    public async Task NotifyMatchingSearchesAsync_Filters_By_MaxPrice()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "buyer-1", Name = "Cheap stuff", MaxPrice = 30m });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<ISavedSearchNotifier>();
        var svc = CreateService(db, notifierMock);
        var item = MakeItemDto("seller-1", price: 50m);

        await svc.NotifyMatchingSearchesAsync(item);

        notifierMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()), Times.Never);
    }

    [Fact]
    public async Task NotifyMatchingSearchesAsync_Matches_Description_SearchTerm()
    {
        await using var db = CreateDb();
        db.SavedSearches.Add(new SavedSearch { UserId = "buyer-1", Name = "Maclaren", SearchTerm = "maclaren" });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<ISavedSearchNotifier>();
        notifierMock.Setup(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<SavedSearchMatchNotification>()))
                    .Returns(Task.CompletedTask);

        var svc = CreateService(db, notifierMock);
        var item = MakeItemDto("seller-1", title: "Baby Stroller", description: "Maclaren Quest, excellent shape");

        await svc.NotifyMatchingSearchesAsync(item);

        notifierMock.Verify(n => n.NotifyAsync("buyer-1", It.IsAny<SavedSearchMatchNotification>()), Times.Once);
    }
}
