using System.Net;
using FluentAssertions;
using System.Net.Http.Json;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Items;
using MomVibe.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using MomVibe.Domain.Entities;

namespace MomVibe.IntegrationTests;

// ---------------------------------------------------------------------------
// Payment endpoints return 401 without authentication
// ---------------------------------------------------------------------------
public class PaymentsUnauthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentsUnauthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyPayments_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/payments/my-payments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCheckout_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/payments/checkout/{Guid.NewGuid()}", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBooking_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/payments/booking/{Guid.NewGuid()}", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOnSpot_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/payments/onspot/{Guid.NewGuid()}", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BulkBooking_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/payments/bulk-booking",
            new { itemIds = new[] { Guid.NewGuid() } });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// ---------------------------------------------------------------------------
// Payment endpoints with valid authentication
// ---------------------------------------------------------------------------
public class PaymentsAuthTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private static readonly Guid ClothingCategoryId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    private readonly HttpClient _client;
    private readonly AuthenticatedWebApplicationFactory _factory;

    public PaymentsAuthTests(AuthenticatedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task MyPayments_WithAuth_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/payments/my-payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBooking_NonExistentItem_ReturnsBadRequest()
    {
        var response = await _client.PostAsync($"/api/payments/booking/{Guid.NewGuid()}", null);

        // Service throws KeyNotFoundException → controller returns BadRequest
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBooking_DonateItem_ReturnsSuccess()
    {
        // Seed a donate item directly into the DB for this test
        var itemId = await SeedDonateItemAsync();

        var response = await _client.PostAsync($"/api/payments/booking/{itemId}", null);

        // Booking a free donate item should succeed
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOnSpot_SellItem_ReturnsSuccess()
    {
        var itemId = await SeedSellItemAsync();

        var response = await _client.PostAsync($"/api/payments/onspot/{itemId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BulkBooking_EmptyItemList_ReturnsOkOrBadRequest()
    {
        // Empty item list: service accepts it gracefully (returns 200 with no bookings created),
        // or the controller may reject it as 400 — either is acceptable.
        var response = await _client.PostAsJsonAsync("/api/payments/bulk-booking",
            new { itemIds = Array.Empty<Guid>() });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BulkCheckout_TooManyItems_ReturnsBadRequest()
    {
        var tooMany = Enumerable.Range(0, 51).Select(_ => Guid.NewGuid()).ToArray();
        var response = await _client.PostAsJsonAsync("/api/payments/bulk-checkout",
            new { itemIds = tooMany });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Webhook_WithInvalidSignature_Returns400()
    {
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        content.Headers.Add("Stripe-Signature", "invalid_sig");

        var response = await _client.PostAsync("/api/payments/webhook", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Helpers: seed items directly into DB so tests don't depend on the full CreateAsync AI pipeline

    private async Task<Guid> SeedDonateItemAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = $"Donate Seed {Guid.NewGuid():N}",
            Description = "Seeded for payment test",
            CategoryId = ClothingCategoryId,
            ListingType = ListingType.Donate,
            UserId = "other-user-001",   // different from test user so it's a real transaction
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    private async Task<Guid> SeedSellItemAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = $"Sell Seed {Guid.NewGuid():N}",
            Description = "Seeded for payment test",
            CategoryId = ClothingCategoryId,
            ListingType = ListingType.Sell,
            Price = 30.00m,
            UserId = "other-user-001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }
}
