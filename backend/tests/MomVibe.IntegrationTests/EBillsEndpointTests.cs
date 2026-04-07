using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Application.DTOs.Payments;

namespace MomVibe.IntegrationTests;

/// <summary>
/// Tests for unauthenticated access — must return 401 via the real JWT policy.
/// Uses CustomWebApplicationFactory so no fake auth scheme is registered.
/// </summary>
public class EBillsUnauthenticatedTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EBillsUnauthenticatedTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyEBills_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/ebills");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEBill_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync($"/api/ebills/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendEBill_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/ebills/{Guid.NewGuid()}/resend", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

/// <summary>
/// Tests for authenticated access — uses AuthenticatedWebApplicationFactory so every request
/// is automatically treated as authenticated (no real JWT token needed).
/// </summary>
public class EBillsAuthenticatedTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EBillsAuthenticatedTests(AuthenticatedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyEBills_WithAuth_ReturnsEmptyList_ForNewUser()
    {
        var response = await _client.GetAsync("/api/ebills");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bills = await response.Content.ReadFromJsonAsync<List<EBillDto>>();
        bills.Should().NotBeNull();
        bills.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEBill_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/ebills/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendEBill_NonExistentId_Returns404()
    {
        var response = await _client.PostAsync($"/api/ebills/{Guid.NewGuid()}/resend", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
