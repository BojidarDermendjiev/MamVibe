using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for EBillService using an EF Core InMemory database
/// so that LINQ Include/Where/FirstOrDefaultAsync behave exactly as production.
/// IEmailService and UserManager are Moq mocks — their interactions are verified.
/// </summary>
public class EBillServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"EBillTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().Build();
        return new ApplicationDbContext(options, config);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static EBillService CreateService(
        ApplicationDbContext db,
        Mock<IEmailService> emailMock,
        Mock<UserManager<ApplicationUser>>? umMock = null)
    {
        umMock ??= CreateUserManagerMock();
        return new EBillService(
            db,
            CreateMapper(),
            emailMock.Object,
            umMock.Object,
            NullLogger<EBillService>.Instance);
    }

    /// <summary>Seeds a seller, buyer, item, and payment; returns the payment.</summary>
    private static async Task<Payment> SeedPaymentAsync(
        ApplicationDbContext db,
        string buyerId = "buyer-1",
        string sellerId = "seller-1",
        ListingType listingType = ListingType.Sell,
        PaymentStatus status = PaymentStatus.Completed,
        string? eBillNumber = null)
    {
        if (!db.Users.Any(u => u.Id == sellerId))
            db.Users.Add(new ApplicationUser { Id = sellerId, DisplayName = "Seller", Email = "seller@test.com", UserName = sellerId });
        if (!db.Users.Any(u => u.Id == buyerId))
            db.Users.Add(new ApplicationUser { Id = buyerId, DisplayName = "Buyer", Email = "buyer@test.com", UserName = buyerId });

        var item = new Item { Title = "Baby Clothes", Description = "desc", UserId = sellerId, ListingType = listingType, IsActive = true };
        db.Items.Add(item);

        var payment = new Payment
        {
            ItemId = item.Id,
            BuyerId = buyerId,
            SellerId = sellerId,
            Amount = 25m,
            PaymentMethod = PaymentMethod.Card,
            Status = status,
            EBillNumber = eBillNumber
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    // =========================================================================
    // GetMyEBillsAsync
    // =========================================================================

    [Fact]
    public async Task GetMyEBillsAsync_Returns_Completed_Sell_Payments_For_Buyer()
    {
        await using var db = CreateDb();
        await SeedPaymentAsync(db, buyerId: "buyer-1");

        var svc = CreateService(db, new Mock<IEmailService>());
        var result = await svc.GetMyEBillsAsync("buyer-1");

        result.Should().HaveCount(1);
        result[0].Amount.Should().Be(25m);
        result[0].BuyerId.Should().Be("buyer-1");
    }

    [Fact]
    public async Task GetMyEBillsAsync_Excludes_Pending_Payments()
    {
        await using var db = CreateDb();
        await SeedPaymentAsync(db, buyerId: "buyer-1", status: PaymentStatus.Pending);

        var svc = CreateService(db, new Mock<IEmailService>());
        var result = await svc.GetMyEBillsAsync("buyer-1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyEBillsAsync_Excludes_Donate_Items()
    {
        await using var db = CreateDb();
        await SeedPaymentAsync(db, buyerId: "buyer-1", listingType: ListingType.Donate);

        var svc = CreateService(db, new Mock<IEmailService>());
        var result = await svc.GetMyEBillsAsync("buyer-1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyEBillsAsync_Excludes_Other_Buyers_Payments()
    {
        await using var db = CreateDb();
        await SeedPaymentAsync(db, buyerId: "buyer-1");

        var svc = CreateService(db, new Mock<IEmailService>());
        var result = await svc.GetMyEBillsAsync("buyer-2");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyEBillsAsync_Returns_Multiple_Bills_Ordered_By_Date_Descending()
    {
        await using var db = CreateDb();
        await SeedPaymentAsync(db, buyerId: "buyer-1");
        await SeedPaymentAsync(db, buyerId: "buyer-1", sellerId: "seller-2");

        var svc = CreateService(db, new Mock<IEmailService>());
        var result = await svc.GetMyEBillsAsync("buyer-1");

        result.Should().HaveCount(2);
        result[0].IssuedAt.Should().BeOnOrAfter(result[1].IssuedAt);
    }

    // =========================================================================
    // GetEBillAsync
    // =========================================================================

    [Fact]
    public async Task GetEBillAsync_Returns_EBill_For_Correct_Owner()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db, buyerId: "buyer-1");

        var svc = CreateService(db, new Mock<IEmailService>());
        var bill = await svc.GetEBillAsync(payment.Id, "buyer-1");

        bill.Should().NotBeNull();
        bill.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task GetEBillAsync_Throws_UnauthorizedAccess_For_Wrong_User()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db, buyerId: "buyer-1");

        var svc = CreateService(db, new Mock<IEmailService>());

        var act = async () => await svc.GetEBillAsync(payment.Id, "buyer-WRONG");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetEBillAsync_Throws_KeyNotFound_For_Missing_Payment()
    {
        await using var db = CreateDb();

        var svc = CreateService(db, new Mock<IEmailService>());

        var act = async () => await svc.GetEBillAsync(Guid.NewGuid(), "buyer-1");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // IssueEBillAsync
    // =========================================================================

    [Fact]
    public async Task IssueEBillAsync_Assigns_EBillNumber_And_Sends_Email()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db);
        var emailMock = new Mock<IEmailService>();

        var svc = CreateService(db, emailMock);
        await svc.IssueEBillAsync(payment.Id, "buyer@test.com");

        var updated = await db.Payments.FindAsync(payment.Id);
        updated!.EBillNumber.Should().NotBeNullOrEmpty()
            .And.StartWith("MV-");

        emailMock.Verify(
            e => e.SendEmailAsync(
                "buyer@test.com",
                It.Is<string>(s => s.Contains("MV-")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task IssueEBillAsync_Is_Idempotent_When_EBillNumber_Already_Set()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db, eBillNumber: "MV-2026-EXISTING");
        var emailMock = new Mock<IEmailService>();

        var svc = CreateService(db, emailMock);
        await svc.IssueEBillAsync(payment.Id, "buyer@test.com");

        // EBillNumber must not change
        var updated = await db.Payments.FindAsync(payment.Id);
        updated!.EBillNumber.Should().Be("MV-2026-EXISTING");

        // Email must NOT be sent a second time
        emailMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task IssueEBillAsync_Silently_Returns_For_Missing_Payment()
    {
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();
        var svc = CreateService(db, emailMock);

        // Should not throw
        await svc.IssueEBillAsync(Guid.NewGuid(), "any@test.com");

        emailMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // =========================================================================
    // ResendEBillEmailAsync
    // =========================================================================

    [Fact]
    public async Task ResendEBillEmailAsync_Sends_Email_Exactly_Once()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db, buyerId: "buyer-1");
        var emailMock = new Mock<IEmailService>();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByIdAsync("buyer-1"))
            .ReturnsAsync(new ApplicationUser { Id = "buyer-1", Email = "buyer@test.com", DisplayName = "Buyer", UserName = "buyer-1" });

        var svc = CreateService(db, emailMock, umMock);
        await svc.ResendEBillEmailAsync(payment.Id, "buyer-1");

        emailMock.Verify(
            e => e.SendEmailAsync("buyer@test.com", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendEBillEmailAsync_Throws_Unauthorized_For_Wrong_User()
    {
        await using var db = CreateDb();
        var payment = await SeedPaymentAsync(db, buyerId: "buyer-1");
        var svc = CreateService(db, new Mock<IEmailService>());

        var act = async () => await svc.ResendEBillEmailAsync(payment.Id, "buyer-WRONG");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
