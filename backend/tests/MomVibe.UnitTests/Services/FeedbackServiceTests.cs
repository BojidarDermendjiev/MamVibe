using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MomVibe.Application.DTOs.Feedbacks;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for FeedbackService using EF Core InMemory.
/// Users are always seeded before feedback entries to satisfy FK constraints.
/// The service uses two queries: one tracked (FindAsync) and one with Include+User.
/// </summary>
public class FeedbackServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"FeedbackTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static FeedbackService CreateService(ApplicationDbContext db) =>
        new FeedbackService(db, CreateMapper());

    /// <summary>
    /// Seeds a user and a feedback entry. Users must be seeded before feedbacks
    /// to avoid the InMemory Include+User navigation bug when calling GetAllAsync.
    /// </summary>
    private static async Task<Feedback> SeedFeedbackAsync(
        ApplicationDbContext db,
        string userId = "user-1",
        int rating = 4,
        FeedbackCategory category = FeedbackCategory.Praise,
        string content = "Great platform!")
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = "Feedback User",
                Email = $"{userId}@test.com",
                UserName = userId
            });

        var feedback = new Feedback
        {
            UserId = userId,
            Rating = rating,
            Category = category,
            Content = content,
            IsContactable = false
        };
        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync();
        return feedback;
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_Saves_Feedback_With_Correct_Properties()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateFeedbackDto
        {
            Rating = 5,
            Category = FeedbackCategory.Praise,
            Content = "Excellent service, very happy!",
            IsContactable = true
        };

        var result = await svc.CreateAsync(dto, "u1");

        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Category.Should().Be(FeedbackCategory.Praise);
        result.IsContactable.Should().BeTrue();

        var saved = await db.Feedbacks.FirstOrDefaultAsync(f => f.UserId == "u1");
        saved.Should().NotBeNull();
        saved!.Content.Should().Be("Excellent service, very happy!");
    }

    [Fact]
    public async Task CreateAsync_Saves_Feedback_With_BugReport_Category()
    {
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "u1", DisplayName = "User", Email = "u@test.com", UserName = "u1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var dto = new CreateFeedbackDto
        {
            Rating = 2,
            Category = FeedbackCategory.BugReport,
            Content = "The search filter is broken for me.",
            IsContactable = false
        };

        var result = await svc.CreateAsync(dto, "u1");

        result.Category.Should().Be(FeedbackCategory.BugReport);
        result.Rating.Should().Be(2);
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_Returns_Paged_Feedback_With_Correct_TotalCount()
    {
        await using var db = CreateDb();
        await SeedFeedbackAsync(db, userId: "u1", content: "First feedback");
        await SeedFeedbackAsync(db, userId: "u2", content: "Second feedback");
        await SeedFeedbackAsync(db, userId: "u3", content: "Third feedback");

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(page: 1, pageSize: 2);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Correct_Second_Page()
    {
        await using var db = CreateDb();
        await SeedFeedbackAsync(db, userId: "u1");
        await SeedFeedbackAsync(db, userId: "u2");
        await SeedFeedbackAsync(db, userId: "u3");

        var svc = CreateService(db);
        var result = await svc.GetAllAsync(page: 2, pageSize: 2);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Empty_When_No_Feedbacks_Exist()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var result = await svc.GetAllAsync();

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_Removes_Feedback_When_Owner_Deletes()
    {
        await using var db = CreateDb();
        var feedback = await SeedFeedbackAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(feedback.Id, "u1");

        var deleted = await db.Feedbacks.FindAsync(feedback.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Removes_Feedback_When_Admin_Deletes()
    {
        await using var db = CreateDb();
        var feedback = await SeedFeedbackAsync(db, userId: "u1");

        var svc = CreateService(db);
        await svc.DeleteAsync(feedback.Id, "admin-user", isAdmin: true);

        var deleted = await db.Feedbacks.FindAsync(feedback.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Throws_Unauthorized_When_Non_Owner_Non_Admin_Deletes()
    {
        await using var db = CreateDb();
        var feedback = await SeedFeedbackAsync(db, userId: "u1");

        var svc = CreateService(db);
        var act = async () => await svc.DeleteAsync(feedback.Id, "other-user", isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*own feedback*");
    }

    [Fact]
    public async Task DeleteAsync_Throws_KeyNotFound_For_Missing_Feedback()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var act = async () => await svc.DeleteAsync(Guid.NewGuid(), "u1");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}
