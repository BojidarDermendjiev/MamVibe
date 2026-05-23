using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MomVibe.Application.DTOs.UserRatings;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for UserRatingService using EF Core InMemory.
/// Users are seeded before PurchaseRequests and UserRatings.
/// CreateAsync uses Include(r => r.Rater) after save, so the rater user must be
/// seeded beforehand to avoid the InMemory Include+User navigation bug.
/// </summary>
public class UserRatingServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"UserRatingTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static UserRatingService CreateService(ApplicationDbContext db) =>
        new UserRatingService(db, CreateMapper());

    private static void SeedUser(ApplicationDbContext db, string userId, string displayName = "Test User")
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = displayName,
                Email = $"{userId}@test.com",
                UserName = userId
            });
    }

    /// <summary>
    /// Seeds an item, a completed PurchaseRequest, and the related users.
    /// Users are seeded before the item and request to avoid InMemory navigation bugs.
    /// </summary>
    private static async Task<PurchaseRequest> SeedCompletedPurchaseAsync(
        ApplicationDbContext db,
        string buyerId = "buyer-1",
        string sellerId = "seller-1",
        PurchaseRequestStatus status = PurchaseRequestStatus.Completed)
    {
        SeedUser(db, sellerId, "Seller");
        SeedUser(db, buyerId, "Buyer");

        var item = new Item
        {
            Title = "Baby Jacket",
            Description = "Warm jacket",
            UserId = sellerId,
            IsActive = true,
            ListingType = ListingType.Sell,
            Price = 40m
        };
        db.Items.Add(item);
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

        return request;
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Saves_Rating_With_Correct_Properties()
    {
        await using var db = CreateDb();
        var request = await SeedCompletedPurchaseAsync(db, buyerId: "buyer-1", sellerId: "seller-1");

        var svc = CreateService(db);
        var dto = new CreateUserRatingDto { Rating = 5, Comment = "Great seller!" };
        var result = await svc.CreateAsync("buyer-1", request.Id, dto);

        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Great seller!");

        var saved = await db.UserRatings.FirstOrDefaultAsync(r => r.PurchaseRequestId == request.Id);
        saved.Should().NotBeNull();
        saved!.RaterId.Should().Be("buyer-1");
        saved.RatedUserId.Should().Be("seller-1");
    }

    [Fact]
    public async Task CreateAsync_Throws_Unauthorized_When_Non_Buyer_Rates()
    {
        await using var db = CreateDb();
        var request = await SeedCompletedPurchaseAsync(db, buyerId: "buyer-1", sellerId: "seller-1");

        var svc = CreateService(db);
        var act = async () => await svc.CreateAsync("seller-1", request.Id, new CreateUserRatingDto { Rating = 3 });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*buyer*");
    }

    [Fact]
    public async Task CreateAsync_Throws_InvalidOperation_When_Purchase_Not_Completed()
    {
        await using var db = CreateDb();
        var request = await SeedCompletedPurchaseAsync(db, buyerId: "buyer-1", sellerId: "seller-1",
            status: PurchaseRequestStatus.Accepted);

        var svc = CreateService(db);
        var act = async () => await svc.CreateAsync("buyer-1", request.Id, new CreateUserRatingDto { Rating = 5 });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public async Task CreateAsync_Throws_InvalidOperation_On_Duplicate_Rating()
    {
        await using var db = CreateDb();
        var request = await SeedCompletedPurchaseAsync(db, buyerId: "buyer-1", sellerId: "seller-1");

        // Seed an existing rating for the same purchase request
        db.UserRatings.Add(new UserRating
        {
            RaterId = "buyer-1",
            RatedUserId = "seller-1",
            PurchaseRequestId = request.Id,
            Rating = 4
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = async () => await svc.CreateAsync("buyer-1", request.Id, new CreateUserRatingDto { Rating = 5 });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been rated*");
    }

    [Fact]
    public async Task CreateAsync_Throws_KeyNotFound_For_Missing_PurchaseRequest()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.CreateAsync("buyer-1", Guid.NewGuid(), new CreateUserRatingDto { Rating = 4 });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // GetSummaryAsync
    // =========================================================================

    [Fact]
    public async Task GetSummaryAsync_Returns_Correct_Average_And_Count()
    {
        await using var db = CreateDb();
        SeedUser(db, "seller-1");
        SeedUser(db, "buyer-1");
        SeedUser(db, "buyer-2");
        await db.SaveChangesAsync();

        db.UserRatings.Add(new UserRating { RaterId = "buyer-1", RatedUserId = "seller-1", PurchaseRequestId = Guid.NewGuid(), Rating = 4 });
        db.UserRatings.Add(new UserRating { RaterId = "buyer-2", RatedUserId = "seller-1", PurchaseRequestId = Guid.NewGuid(), Rating = 2 });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var (average, count) = await svc.GetSummaryAsync("seller-1");

        count.Should().Be(2);
        average.Should().Be(3.0, "average of 4 and 2 is 3");
    }

    [Fact]
    public async Task GetSummaryAsync_Returns_Null_Average_And_Zero_Count_When_No_Ratings()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var (average, count) = await svc.GetSummaryAsync("seller-with-no-ratings");

        average.Should().BeNull();
        count.Should().Be(0);
    }

    // =========================================================================
    // GetForUserAsync
    // =========================================================================

    [Fact]
    public async Task GetForUserAsync_Returns_All_Ratings_For_Seller()
    {
        await using var db = CreateDb();
        SeedUser(db, "seller-1");
        SeedUser(db, "buyer-1");
        SeedUser(db, "buyer-2");
        await db.SaveChangesAsync();

        db.UserRatings.Add(new UserRating { RaterId = "buyer-1", RatedUserId = "seller-1", PurchaseRequestId = Guid.NewGuid(), Rating = 5 });
        db.UserRatings.Add(new UserRating { RaterId = "buyer-2", RatedUserId = "seller-1", PurchaseRequestId = Guid.NewGuid(), Rating = 3 });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var ratings = (await svc.GetForUserAsync("seller-1")).ToList();

        ratings.Should().HaveCount(2);
        ratings.Should().AllSatisfy(r => r.Rating.Should().BeInRange(1, 5));
    }

    [Fact]
    public async Task GetForUserAsync_Returns_Empty_When_Seller_Has_No_Ratings()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var ratings = (await svc.GetForUserAsync("unknown-seller")).ToList();

        ratings.Should().BeEmpty();
    }
}
