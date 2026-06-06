using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MomVibe.Application.DTOs.ChildFriendlyPlaces;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for ChildFriendlyPlaceService using EF Core InMemory.
/// Users are always seeded before places to satisfy FK constraints required
/// by InMemory Include+User navigation.
/// </summary>
public class ChildFriendlyPlaceServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ChildFriendlyPlaceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ChildFriendlyPlaceService CreateService(ApplicationDbContext db) =>
        new ChildFriendlyPlaceService(db);

    /// <summary>Seeds a user and an approved child-friendly place. Returns the place entity.</summary>
    private static async Task<ChildFriendlyPlace> SeedPlaceAsync(
        ApplicationDbContext db,
        string userId = "user-1",
        bool isApproved = true,
        string city = "Sofia",
        PlaceType placeType = PlaceType.Playground)
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = "Place Submitter",
                Email = $"{userId}@test.com",
                UserName = userId
            });

        var place = new ChildFriendlyPlace
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Fun Place",
            Description = "A great place for kids.",
            City = city,
            PlaceType = placeType,
            IsApproved = isApproved
        };
        db.ChildFriendlyPlaces.Add(place);
        await db.SaveChangesAsync();
        return place;
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_Returns_Only_Approved_Places()
    {
        await using var db = CreateDb();
        await SeedPlaceAsync(db, userId: "u1", isApproved: true);
        await SeedPlaceAsync(db, userId: "u2", isApproved: false);

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].IsApproved.Should().BeTrue();
    }

    [Fact(Skip = "EF.Functions.ILike is a PostgreSQL-specific operator not supported by the InMemory provider. Filter correctness is verified by integration tests against a real Postgres instance.")]
    public async Task GetAllAsync_Filters_By_City()
    {
        await using var db = CreateDb();
        await SeedPlaceAsync(db, userId: "u1", city: "Sofia");
        await SeedPlaceAsync(db, userId: "u2", city: "Plovdiv");

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync(city: "sofia")).ToList();

        result.Should().HaveCount(1);
        result[0].City.Should().Be("Sofia");
    }

    [Fact]
    public async Task GetAllAsync_Filters_By_PlaceType()
    {
        await using var db = CreateDb();
        await SeedPlaceAsync(db, userId: "u1", placeType: PlaceType.Playground);
        await SeedPlaceAsync(db, userId: "u2", placeType: PlaceType.Restaurant);

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync(placeType: PlaceType.Restaurant)).ToList();

        result.Should().HaveCount(1);
        result[0].PlaceType.Should().Be(PlaceType.Restaurant);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Empty_When_No_Approved_Places_Exist()
    {
        await using var db = CreateDb();
        await SeedPlaceAsync(db, userId: "u1", isApproved: false);

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync()).ToList();

        result.Should().BeEmpty();
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_Returns_Dto_For_Existing_Place()
    {
        await using var db = CreateDb();
        var place = await SeedPlaceAsync(db, userId: "u1");

        var svc = CreateService(db);
        var result = await svc.GetByIdAsync(place.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(place.Id);
        result.Name.Should().Be("Fun Place");
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_For_Missing_Id()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Saves_Place_With_Correct_UserId()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "City Garden",
            Description = "A nice park in the city centre.",
            City = "Varna",
            PlaceType = PlaceType.Park
        };

        var result = await svc.CreateAsync("u1", dto);

        result.Should().NotBeNull();
        result.UserId.Should().Be("u1");
        result.Name.Should().Be("City Garden");
        result.IsApproved.Should().BeFalse("new places start as pending");
    }

    [Fact]
    public async Task CreateAsync_Persists_Place_To_Database()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Beach Point",
            Description = "Sandy beach ideal for families.",
            City = "Burgas",
            PlaceType = PlaceType.Beach
        };

        await svc.CreateAsync("u1", dto);

        var saved = await db.ChildFriendlyPlaces.FirstOrDefaultAsync(p => p.Name == "Beach Point");
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be("u1");
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_Place_When_Owner_Deletes()
    {
        await using var db = CreateDb();
        var place = await SeedPlaceAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(place.Id, "u1");

        var deleted = await db.ChildFriendlyPlaces.FindAsync(place.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Removes_Place_When_Admin_Deletes()
    {
        await using var db = CreateDb();
        var place = await SeedPlaceAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(place.Id, "admin-99", isAdmin: true);

        var deleted = await db.ChildFriendlyPlaces.FindAsync(place.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_Unauthorized_For_Non_Owner_Non_Admin()
    {
        await using var db = CreateDb();
        var place = await SeedPlaceAsync(db, userId: "u1");

        var svc = CreateService(db);
        var act = async () => await svc.DeleteAsync(place.Id, "intruder-user", isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_Throws_KeyNotFound_For_Missing_Place()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.DeleteAsync(Guid.NewGuid(), "u1");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    // =========================================================================
    // ApproveAsync
    // =========================================================================

    [Fact]
    public async Task ApproveAsync_Sets_IsApproved_True()
    {
        await using var db = CreateDb();
        var place = await SeedPlaceAsync(db, userId: "u1", isApproved: false);

        var svc = CreateService(db);
        await svc.ApproveAsync(place.Id);

        var updated = await db.ChildFriendlyPlaces.FindAsync(place.Id);
        updated!.IsApproved.Should().BeTrue();
    }
}
