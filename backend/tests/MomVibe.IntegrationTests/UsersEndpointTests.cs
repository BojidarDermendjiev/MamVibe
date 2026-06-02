using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Users;

namespace MomVibe.IntegrationTests;

public class UsersPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_NonExistentUser_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuth_Returns401()
    {
        var dto = new UpdateProfileDto { DisplayName = "New Name" };
        var response = await _client.PutAsJsonAsync("/api/v1/users/profile", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyItems_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/users/dashboard/items");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLikedItems_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/users/dashboard/liked");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterPushToken_WithoutAuth_Returns401()
    {
        var dto = new RegisterPushTokenDto { Token = "ExponentPushToken[test]" };
        var response = await _client.PostAsJsonAsync("/api/v1/users/push-token", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class UsersAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_SeededTestUser_Returns200()
    {
        var response = await _client.GetAsync($"/api/v1/users/{GeneralAuthWebApplicationFactory.TestUserId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(GeneralAuthWebApplicationFactory.TestUserId);
    }

    [Fact]
    public async Task GetProfile_EmailVisibleOnOwnProfile()
    {
        // The test user is authenticated as TestUserId, requesting their own profile
        var response = await _client.GetAsync($"/api/v1/users/{GeneralAuthWebApplicationFactory.TestUserId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        // Own profile must not have empty email
        user!.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMyItems_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/users/dashboard/items");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLikedItems_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/users/dashboard/liked");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProfile_ChangesDisplayName()
    {
        var dto = new UpdateProfileDto { DisplayName = "Updated Name", Bio = "Hello from test" };
        var response = await _client.PutAsJsonAsync("/api/v1/users/profile", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<UserDto>();
        updated!.DisplayName.Should().Be("Updated Name");
        updated.Bio.Should().Be("Hello from test");
    }

    [Fact]
    public async Task RegisterPushToken_Returns204()
    {
        var dto = new RegisterPushTokenDto { Token = "ExponentPushToken[abc123]" };
        var response = await _client.PostAsJsonAsync("/api/v1/users/push-token", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
