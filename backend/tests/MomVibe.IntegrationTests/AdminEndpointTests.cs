using System.Net;
using FluentAssertions;
using System.Net.Http.Json;

using MomVibe.Application.DTOs.Admin;
using MomVibe.Application.DTOs.Common;
using MomVibe.Application.DTOs.Items;

namespace MomVibe.IntegrationTests;

// ---------------------------------------------------------------------------
// Admin endpoints are forbidden for non-admin authenticated users
// ---------------------------------------------------------------------------
public class AdminForbiddenTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminForbiddenTests(AuthenticatedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboard_WithoutAdminRole_Returns403()
    {
        var response = await _client.GetAsync("/api/admin/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithoutAdminRole_Returns403()
    {
        var response = await _client.GetAsync("/api/admin/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingItems_WithoutAdminRole_Returns403()
    {
        var response = await _client.GetAsync("/api/admin/items/pending");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BlockUser_WithoutAdminRole_Returns403()
    {
        var response = await _client.PostAsync($"/api/admin/users/{Guid.NewGuid()}/block", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

// ---------------------------------------------------------------------------
// Admin endpoints return 401 for unauthenticated requests
// ---------------------------------------------------------------------------
public class AdminUnauthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminUnauthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboard_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/admin/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/admin/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// ---------------------------------------------------------------------------
// Admin endpoints work correctly for Admin-role users
// ---------------------------------------------------------------------------
public class AdminAuthorizedTests : IClassFixture<AdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminAuthorizedTests(AdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOkWithStats()
    {
        var response = await _client.GetAsync("/api/admin/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        stats.Should().NotBeNull();
        stats!.TotalUsers.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalItems.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithPagedResult()
    {
        var response = await _client.GetAsync("/api/admin/users?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdminUserDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsers_WithSearch_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/admin/users?search=test&page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingItems_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/admin/items/pending?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveItem_NonExistentItem_Returns404()
    {
        var response = await _client.PostAsync($"/api/admin/items/{Guid.NewGuid()}/approve", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminDeleteItem_NonExistentItem_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/admin/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BlockUser_NonExistentUser_Returns404()
    {
        var response = await _client.PostAsync($"/api/admin/users/{Guid.NewGuid()}/block", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnblockUser_NonExistentUser_Returns404()
    {
        var response = await _client.PostAsync($"/api/admin/users/{Guid.NewGuid()}/unblock", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetModerationHistory_NonExistentItem_ReturnsOkWithEmptyList()
    {
        var response = await _client.GetAsync($"/api/admin/items/{Guid.NewGuid()}/moderation-history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<ModerationLogEntryDto>>();
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboard_ReturnsCachedResult_OnSecondCall()
    {
        var first = await _client.GetAsync("/api/admin/dashboard");
        var second = await _client.GetAsync("/api/admin/dashboard");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats1 = await first.Content.ReadFromJsonAsync<DashboardStatsDto>();
        var stats2 = await second.Content.ReadFromJsonAsync<DashboardStatsDto>();

        // Both responses should be identical (second served from cache)
        stats1!.TotalUsers.Should().Be(stats2!.TotalUsers);
        stats1.TotalItems.Should().Be(stats2.TotalItems);
    }
}
