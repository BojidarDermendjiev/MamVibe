using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Shipping;

namespace MomVibe.IntegrationTests;

public class ShippingPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ShippingPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/shipping/calculate", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShipment_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/shipping/create", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOffices_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/shipping/offices?provider=0");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLabel_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync($"/api/v1/shipping/{Guid.NewGuid()}/label");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrackShipment_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync($"/api/v1/shipping/{Guid.NewGuid()}/track");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MyShipments_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/shipping/my-shipments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class ShippingAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ShippingAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_WithEmptyBody_Returns400()
    {
        // Empty CalculateShippingDto will fail FluentValidation
        var response = await _client.PostAsJsonAsync("/api/v1/shipping/calculate", new CalculateShippingDto());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShipment_WithEmptyBody_Returns400()
    {
        // Minimal CreateShipmentDto with no meaningful data will fail FluentValidation
        var response = await _client.PostAsJsonAsync("/api/v1/shipping/create",
            new CreateShipmentDto { RecipientName = "", RecipientPhone = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MyShipments_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/shipping/my-shipments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetShipmentByPayment_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/shipping/payment/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
