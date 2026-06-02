using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace MomVibe.IntegrationTests;

public class PurchaseRequestsPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PurchaseRequestsPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/purchase-requests", new { ItemId = Guid.NewGuid() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsSeller_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/purchase-requests/as-seller");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsBuyer_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/purchase-requests/as-buyer");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Accept_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/accept", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Decline_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/decline", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class PurchaseRequestsAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PurchaseRequestsAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAsSeller_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/purchase-requests/as-seller");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAsBuyer_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/purchase-requests/as-buyer");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accept_NonExistent_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/accept", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Decline_NonExistent_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/decline", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PaymentChosen_NonExistent_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/payment-chosen",
            new { PaymentMethod = "card" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckBuyer_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/purchase-requests/{Guid.NewGuid()}/buyer-check");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
