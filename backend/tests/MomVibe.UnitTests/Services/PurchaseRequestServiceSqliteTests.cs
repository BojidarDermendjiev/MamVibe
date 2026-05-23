using AutoMapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

using MomVibe.Application.DTOs.PurchaseRequests;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// SQLite-compatible subclass that clears the PostgreSQL-specific
/// <c>NOW() AT TIME ZONE 'UTC'</c> default from <c>ApplicationUserConfiguration</c>
/// so that <c>EnsureCreatedAsync</c> generates valid SQLite DDL.
/// </summary>
internal class SqliteTestDbContext : ApplicationDbContext
{
    public SqliteTestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql(null);
    }
}

/// <summary>
/// Integration-style unit tests for <see cref="PurchaseRequestService.CreateAsync"/> using
/// an in-process SQLite database. InMemory EF Core cannot run <c>ExecuteUpdateAsync</c> with
/// a correlated subquery, so these tests use a real SQL engine to verify the atomic
/// check-and-lock that prevents double-reservation.
/// </summary>
public class PurchaseRequestServiceSqliteTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ApplicationDbContext _db = null!;

    private const string SellerId = "seller-sqlite-001";
    private const string BuyerId = "buyer-sqlite-002";
    private const string OtherBuyerId = "buyer-sqlite-003";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Keep the in-memory SQLite connection open for the lifetime of the test class
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // SqliteTestDbContext neutralises the PostgreSQL-specific default SQL
        _db = new SqliteTestDbContext(options);
        await _db.Database.EnsureCreatedAsync();

        // Seed category (FK required by Item)
        var category = new Category { Id = Guid.NewGuid(), Name = "Baby Clothes", Slug = "baby-clothes" };
        _db.Categories.Add(category);

        // Seed seller and buyer as minimal Identity users
        _db.Users.Add(new ApplicationUser
        {
            Id = SellerId, UserName = "seller@test.com", NormalizedUserName = "SELLER@TEST.COM",
            Email = "seller@test.com", NormalizedEmail = "SELLER@TEST.COM",
            DisplayName = "Test Seller",
            SecurityStamp = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString(),
        });
        _db.Users.Add(new ApplicationUser
        {
            Id = BuyerId, UserName = "buyer@test.com", NormalizedUserName = "BUYER@TEST.COM",
            Email = "buyer@test.com", NormalizedEmail = "BUYER@TEST.COM",
            DisplayName = "Test Buyer",
            SecurityStamp = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString(),
        });
        _db.Users.Add(new ApplicationUser
        {
            Id = OtherBuyerId, UserName = "other@test.com", NormalizedUserName = "OTHER@TEST.COM",
            Email = "other@test.com", NormalizedEmail = "OTHER@TEST.COM",
            DisplayName = "Other Buyer",
            SecurityStamp = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString(),
        });

        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private PurchaseRequestService BuildService()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        var notifier = new Mock<IPurchaseRequestNotifier>();
        notifier.Setup(n => n.NotifySellerAsync(It.IsAny<string>(), It.IsAny<PurchaseRequestDto>()))
                .Returns(Task.CompletedTask);
        var nekorekten = new Mock<INekorektenService>();

        return new PurchaseRequestService(_db, mapper, notifier.Object, nekorekten.Object);
    }

    private async Task<Item> SeedActiveItemAsync(string? sellerId = null)
    {
        var category = await _db.Categories.FirstAsync();
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = $"Test Item {Guid.NewGuid():N}",
            Description = "A test item",
            CategoryId = category.Id,
            UserId = sellerId ?? SellerId,
            ListingType = ListingType.Sell,
            Price = 10m,
            IsActive = true,
            AiModerationStatus = AiModerationStatus.AutoApproved,
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ActiveItem_ReturnsPurchaseRequestDto()
    {
        var item = await SeedActiveItemAsync();
        var svc = BuildService();

        var result = await svc.CreateAsync(item.Id, BuyerId);

        result.Should().NotBeNull();
        result.ItemId.Should().Be(item.Id);
        result.BuyerId.Should().Be(BuyerId);
        result.SellerId.Should().Be(SellerId);
        result.Status.Should().Be(PurchaseRequestStatus.Pending);
    }

    [Fact]
    public async Task CreateAsync_ActiveItem_SetsItemInactiveInDatabase()
    {
        var item = await SeedActiveItemAsync();
        var svc = BuildService();

        await svc.CreateAsync(item.Id, BuyerId);

        var reloaded = await _db.Items.AsNoTracking().FirstAsync(i => i.Id == item.Id);
        reloaded.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ActiveItem_PersistsPurchaseRequest()
    {
        var item = await SeedActiveItemAsync();
        var svc = BuildService();

        var result = await svc.CreateAsync(item.Id, BuyerId);

        var inDb = await _db.PurchaseRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == result.Id);
        inDb.Should().NotBeNull();
        inDb!.Status.Should().Be(PurchaseRequestStatus.Pending);
    }

    // ── Item not found ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_NonExistentItemId_ThrowsKeyNotFoundException()
    {
        var svc = BuildService();
        var act = () => svc.CreateAsync(Guid.NewGuid(), BuyerId);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    // ── Item already inactive ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InactiveItem_ThrowsInvalidOperationException()
    {
        var category = await _db.Categories.FirstAsync();
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Inactive Item",
            Description = "Already sold",
            CategoryId = category.Id,
            UserId = SellerId,
            ListingType = ListingType.Sell,
            IsActive = false,   // already locked
            AiModerationStatus = AiModerationStatus.AutoApproved,
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        var svc = BuildService();
        var act = () => svc.CreateAsync(item.Id, BuyerId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    // ── Existing pending request blocks a second buyer ─────────────────────────

    [Fact]
    public async Task CreateAsync_ItemAlreadyHasPendingRequest_ThrowsInvalidOperationException()
    {
        var item = await SeedActiveItemAsync();

        // First buyer reserves it
        var svc = BuildService();
        await svc.CreateAsync(item.Id, BuyerId);

        // Second buyer tries — the item is now inactive AND has a Pending PR
        var act = () => svc.CreateAsync(item.Id, OtherBuyerId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    // ── Owner cannot request their own item ───────────────────────────────────

    [Fact]
    public async Task CreateAsync_BuyerIsOwner_ThrowsInvalidOperationException()
    {
        var item = await SeedActiveItemAsync(sellerId: SellerId);
        var svc = BuildService();

        // Seller tries to buy their own item
        var act = () => svc.CreateAsync(item.Id, SellerId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*own item*");
    }

    [Fact]
    public async Task CreateAsync_BuyerIsOwner_ReactivatesItemAfterRejection()
    {
        var item = await SeedActiveItemAsync(sellerId: SellerId);
        var svc = BuildService();

        try { await svc.CreateAsync(item.Id, SellerId); } catch (InvalidOperationException) { }

        // Item must be active again (lock was reverted)
        var reloaded = await _db.Items.AsNoTracking().FirstAsync(i => i.Id == item.Id);
        reloaded.IsActive.Should().BeTrue();
    }

    // ── Atomic reservation: second concurrent call loses the race ─────────────

    [Fact]
    public async Task CreateAsync_ConcurrentRequests_OnlyOneSucceeds()
    {
        var item = await SeedActiveItemAsync();

        // Two services backed by the SAME db context (simulates two in-flight requests)
        var svc1 = BuildService();
        var svc2 = BuildService();

        // Run sequentially — SQLite in-memory is single-file so we test the WHERE clause logic
        PurchaseRequestDto? first = null;
        Exception? secondEx = null;

        try { first = await svc1.CreateAsync(item.Id, BuyerId); } catch { }
        try { await svc2.CreateAsync(item.Id, OtherBuyerId); }
        catch (Exception ex) { secondEx = ex; }

        first.Should().NotBeNull("the first caller should have succeeded");
        secondEx.Should().BeOfType<InvalidOperationException>("the second caller should be rejected");

        var prCount = await _db.PurchaseRequests.CountAsync(r => r.ItemId == item.Id);
        prCount.Should().Be(1, "exactly one purchase request should exist");
    }
}
