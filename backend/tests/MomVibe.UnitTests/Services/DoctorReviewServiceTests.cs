using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MomVibe.Application.DTOs.DoctorReviews;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for DoctorReviewService using EF Core InMemory.
/// Users are seeded before reviews to avoid the InMemory Include+User navigation bug.
/// The service uses a manual MapToDto (private static), so AutoMapper is not required;
/// the constructor still accepts IMapper so we pass a real instance.
/// </summary>
public class DoctorReviewServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"DoctorReviewTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static DoctorReviewService CreateService(ApplicationDbContext db) =>
        new DoctorReviewService(db, CreateMapper());

    /// <summary>Seeds a user and an approved doctor review. Returns the review entity.</summary>
    private static async Task<DoctorReview> SeedReviewAsync(
        ApplicationDbContext db,
        string userId = "user-1",
        bool isApproved = true,
        string doctorName = "Dr. Smith",
        string city = "Sofia",
        string specialization = "Pediatrics")
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = "Reviewer",
                Email = $"{userId}@test.com",
                UserName = userId
            });

        var review = new DoctorReview
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DoctorName = doctorName,
            Specialization = specialization,
            City = city,
            Rating = 4,
            Content = "Great doctor.",
            IsApproved = isApproved
        };
        db.DoctorReviews.Add(review);
        await db.SaveChangesAsync();
        return review;
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_Returns_Only_Approved_Reviews()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db, userId: "u1", isApproved: true);
        await SeedReviewAsync(db, userId: "u2", isApproved: false);

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_Filters_By_City()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db, userId: "u1", city: "Sofia");
        await SeedReviewAsync(db, userId: "u2", city: "Plovdiv");

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync(city: "sofia")).ToList();

        result.Should().HaveCount(1);
        result[0].City.Should().Be("Sofia");
    }

    [Fact]
    public async Task GetAllAsync_Filters_By_Specialization()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db, userId: "u1", specialization: "Pediatrics");
        await SeedReviewAsync(db, userId: "u2", specialization: "Cardiology");

        var svc = CreateService(db);
        var result = (await svc.GetAllAsync(specialization: "pediatrics")).ToList();

        result.Should().HaveCount(1);
        result[0].Specialization.Should().Be("Pediatrics");
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_Returns_Dto_For_Existing_Review()
    {
        await using var db = CreateDb();
        var review = await SeedReviewAsync(db, userId: "u1");

        var svc = CreateService(db);
        var result = await svc.GetByIdAsync(review.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
        result.DoctorName.Should().Be("Dr. Smith");
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
    public async Task CreateAsync_Saves_Review_With_Correct_UserId()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Jones",
            Specialization = "Neurology",
            City = "Varna",
            Rating = 5,
            Content = "Excellent specialist."
        };

        var result = await svc.CreateAsync("u1", dto);

        result.Should().NotBeNull();
        result.UserId.Should().Be("u1");
        result.DoctorName.Should().Be("Dr. Jones");
        result.IsApproved.Should().BeFalse("new reviews start pending");
    }

    [Fact]
    public async Task CreateAsync_Persists_Review_To_Database()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Brown",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "Average experience."
        };

        await svc.CreateAsync("u1", dto);

        var saved = await db.DoctorReviews.FirstOrDefaultAsync(r => r.DoctorName == "Dr. Brown");
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be("u1");
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_Review_When_Owner_Deletes()
    {
        await using var db = CreateDb();
        var review = await SeedReviewAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(review.Id, "u1");

        var deleted = await db.DoctorReviews.FindAsync(review.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Removes_Review_When_Admin_Deletes()
    {
        await using var db = CreateDb();
        var review = await SeedReviewAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(review.Id, "admin-user", isAdmin: true);

        var deleted = await db.DoctorReviews.FindAsync(review.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_Unauthorized_For_Non_Owner_Non_Admin()
    {
        await using var db = CreateDb();
        var review = await SeedReviewAsync(db, userId: "u1");

        var svc = CreateService(db);
        var act = async () => await svc.DeleteAsync(review.Id, "other-user", isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_Throws_KeyNotFound_For_Missing_Review()
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
        var review = await SeedReviewAsync(db, userId: "u1", isApproved: false);

        var svc = CreateService(db);
        await svc.ApproveAsync(review.Id);

        var updated = await db.DoctorReviews.FindAsync(review.Id);
        updated!.IsApproved.Should().BeTrue();
    }
}
