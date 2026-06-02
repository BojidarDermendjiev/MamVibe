using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Feedbacks;

namespace MomVibe.IntegrationTests;

public class FeedbackPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FeedbackPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200WithPagedResult()
    {
        var response = await _client.GetAsync("/api/v1/feedback");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithPagination_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/feedback?page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var dto = new CreateFeedbackDto { Rating = 5, Category = FeedbackCategory.Praise, Content = "Great platform!" };
        var response = await _client.PostAsJsonAsync("/api/v1/feedback", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync($"/api/v1/feedback/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class FeedbackAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FeedbackAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var dto = new CreateFeedbackDto
        {
            Rating = 5,
            Category = FeedbackCategory.Praise,
            Content = "This is a great platform for parents!",
            IsContactable = false,
        };

        var response = await _client.PostAsJsonAsync("/api/v1/feedback", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithEmptyContent_Returns400()
    {
        var dto = new CreateFeedbackDto
        {
            Rating = 3,
            Category = FeedbackCategory.Improvement,
            Content = "",  // validator requires NotEmpty
        };

        var response = await _client.PostAsJsonAsync("/api/v1/feedback", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInvalidRating_Returns400()
    {
        var dto = new CreateFeedbackDto
        {
            Rating = 0,  // validator requires 1–5
            Category = FeedbackCategory.BugReport,
            Content = "This content is long enough to pass validation",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/feedback", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_NonExistentFeedback_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/v1/feedback/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ThenDelete_Returns204()
    {
        var dto = new CreateFeedbackDto
        {
            Rating = 4,
            Category = FeedbackCategory.FeatureRequest,
            Content = "Very intuitive and easy to use application",
            IsContactable = true,
        };

        var createResp = await _client.PostAsJsonAsync("/api/v1/feedback", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<FeedbackDto>();

        var deleteResp = await _client.DeleteAsync($"/api/v1/feedback/{created!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
