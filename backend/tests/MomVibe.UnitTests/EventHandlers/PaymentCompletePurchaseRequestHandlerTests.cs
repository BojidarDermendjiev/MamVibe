using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using MomVibe.Application.Events;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Infrastructure.EventHandlers;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.UnitTests.EventHandlers;

/// <summary>
/// Pinning tests for <see cref="PaymentCompletePurchaseRequestHandler"/>. The behaviour
/// originally lived inline inside <c>PaymentService.HandleWebhookAsync</c>; these tests
/// guarantee the move to a MediatR handler did not regress it.
/// </summary>
public class PaymentCompletePurchaseRequestHandlerTests
{
    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PaymentCpr_{Guid.NewGuid()}")
            .Options);

    private static PaymentCompletePurchaseRequestHandler CreateHandler(ApplicationDbContext db) =>
        new(db, NullLogger<PaymentCompletePurchaseRequestHandler>.Instance);

    private static async Task<(Item item, Payment payment, PurchaseRequest request)> SeedAsync(
        ApplicationDbContext db,
        PurchaseRequestStatus requestStatus = PurchaseRequestStatus.Accepted)
    {
        db.Users.Add(new ApplicationUser { Id = "seller-1", DisplayName = "Seller", Email = "s@t.com", UserName = "seller-1" });
        db.Users.Add(new ApplicationUser { Id = "buyer-1",  DisplayName = "Buyer",  Email = "b@t.com", UserName = "buyer-1" });
        var category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = $"cat-{Guid.NewGuid()}" };
        db.Categories.Add(category);

        var item = new Item
        {
            Title = "Item",
            Description = "Description",
            UserId = "seller-1",
            CategoryId = category.Id,
            ListingType = ListingType.Sell,
            IsActive = true,
            IsReserved = true,
            IsSold = false,
            Price = 25m
        };
        db.Items.Add(item);
        var payment = new Payment
        {
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            ItemId = item.Id,
            Amount = 25m,
            Status = PaymentStatus.Completed,
            PaymentMethod = PaymentMethod.Card,
        };
        db.Payments.Add(payment);
        var request = new PurchaseRequest
        {
            ItemId = item.Id,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Status = requestStatus,
        };
        db.PurchaseRequests.Add(request);
        await db.SaveChangesAsync();
        return (item, payment, request);
    }

    [Fact]
    public async Task Handle_Marks_Accepted_PurchaseRequest_Completed_And_Item_Sold()
    {
        await using var db = CreateDb();
        var (item, payment, request) = await SeedAsync(db);

        await CreateHandler(db).Handle(new PaymentCompletedEvent(payment.Id), CancellationToken.None);

        var updatedRequest = await db.PurchaseRequests.FindAsync(request.Id);
        var updatedItem = await db.Items.FindAsync(item.Id);
        updatedRequest!.Status.Should().Be(PurchaseRequestStatus.Completed);
        updatedItem!.IsSold.Should().BeTrue();
        updatedItem.IsActive.Should().BeFalse();
        updatedItem.IsReserved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Ignores_PurchaseRequest_That_Is_Not_Accepted()
    {
        await using var db = CreateDb();
        var (_, payment, request) = await SeedAsync(db, PurchaseRequestStatus.Pending);

        await CreateHandler(db).Handle(new PaymentCompletedEvent(payment.Id), CancellationToken.None);

        var unchanged = await db.PurchaseRequests.FindAsync(request.Id);
        unchanged!.Status.Should().Be(PurchaseRequestStatus.Pending);
    }

    [Fact]
    public async Task Handle_Skips_Bundle_Payments()
    {
        // Bundle payments have ItemId = null + BundleId set; bundle completion is owned
        // by PaymentService directly because it touches every member item atomically.
        await using var db = CreateDb();
        db.Users.Add(new ApplicationUser { Id = "seller-1", DisplayName = "S", Email = "s@t.com", UserName = "s" });
        db.Users.Add(new ApplicationUser { Id = "buyer-1",  DisplayName = "B", Email = "b@t.com", UserName = "b" });
        var bundleId = Guid.NewGuid();
        var payment = new Payment
        {
            BundleId = bundleId,
            ItemId = null,
            BuyerId = "buyer-1",
            SellerId = "seller-1",
            Amount = 50m,
            Status = PaymentStatus.Completed,
            PaymentMethod = PaymentMethod.Card,
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        // Should be a no-op: no purchase request to update, no exception.
        var act = async () => await CreateHandler(db).Handle(new PaymentCompletedEvent(payment.Id), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_Tolerates_Missing_Payment_Row()
    {
        // The Stripe webhook is at-least-once: a stale event for a long-deleted Payment
        // should be ignored without throwing.
        await using var db = CreateDb();
        var act = async () => await CreateHandler(db).Handle(new PaymentCompletedEvent(Guid.NewGuid()), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
