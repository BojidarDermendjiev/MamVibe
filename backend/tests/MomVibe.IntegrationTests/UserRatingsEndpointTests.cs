using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Application.DTOs.UserRatings;

namespace MomVibe.IntegrationTests;

public class UserRatingsPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserRatingsPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRatingsForUser_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}/ratings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRatingSummary_Returns200()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}/ratings/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateRating_WithoutAuth_Returns401()
    {
        var dto = new CreateUserRatingDto { Rating = 5 };
        var response = await _client.PostAsJsonAsync($"/api/purchase-requests/{Guid.NewGuid()}/rating", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class UserRatingsAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserRatingsAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRating_WithRatingBelowOne_Returns400()
    {
        var dto = new CreateUserRatingDto { Rating = 0 };
        var response = await _client.PostAsJsonAsync($"/api/purchase-requests/{Guid.NewGuid()}/rating", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRating_WithRatingAboveFive_Returns400()
    {
        var dto = new CreateUserRatingDto { Rating = 6 };
        var response = await _client.PostAsJsonAsync($"/api/purchase-requests/{Guid.NewGuid()}/rating", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRating_ForNonExistentPurchaseRequest_Returns404()
    {
        var dto = new CreateUserRatingDto { Rating = 5, Comment = "Great seller!" };
        var response = await _client.PostAsJsonAsync($"/api/purchase-requests/{Guid.NewGuid()}/rating", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRatingSummary_ForUserWithNoRatings_ReturnsZeroCounts()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}/ratings/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<RatingSummary>();
        body.Should().NotBeNull();
        body!.Count.Should().Be(0);
    }

    private record RatingSummary(double? Average, int Count);
}
