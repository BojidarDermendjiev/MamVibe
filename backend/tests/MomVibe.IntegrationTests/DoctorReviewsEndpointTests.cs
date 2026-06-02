using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using MomVibe.Application.DTOs.DoctorReviews;

namespace MomVibe.IntegrationTests;

public class DoctorReviewsPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DoctorReviewsPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/doctor-reviews");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithFilters_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/doctor-reviews?city=Sofia&specialization=Pediatrics&page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/doctor-reviews/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMine_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/doctor-reviews/mine");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var dto = new CreateDoctorReviewDto { DoctorName = "Dr. Smith", Specialization = "Pediatrics", City = "Sofia", Rating = 5, Content = "Great doctor" };
        var response = await _client.PostAsJsonAsync("/api/v1/doctor-reviews", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync($"/api/v1/doctor-reviews/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class DoctorReviewsAuthTests : IClassFixture<GeneralAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DoctorReviewsAuthTests(GeneralAuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateDoctorReviewDto ValidDto(string doctorName = "Dr. Petrov") => new()
    {
        DoctorName = doctorName,
        Specialization = "Pediatrics",
        City = "Sofia",
        Rating = 5,
        Content = "Excellent doctor — very thorough and kind to children. Highly recommend!",
        ClinicName = "City Medical Center",
    };

    [Fact]
    public async Task GetMine_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/doctor-reviews/mine");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/doctor-reviews", ValidDto());
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var review = await response.Content.ReadFromJsonAsync<DoctorReviewDto>();
        review.Should().NotBeNull();
        review!.DoctorName.Should().Be("Dr. Petrov");
        review.City.Should().Be("Sofia");
    }

    [Fact]
    public async Task GetById_AfterCreate_Returns200()
    {
        var createResp = await _client.PostAsJsonAsync("/api/v1/doctor-reviews", ValidDto($"Dr. {Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<DoctorReviewDto>();

        var getResp = await _client.GetAsync($"/api/v1/doctor-reviews/{created!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/v1/doctor-reviews/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OwnReview_Returns204()
    {
        var createResp = await _client.PostAsJsonAsync("/api/v1/doctor-reviews", ValidDto($"Dr. Del-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<DoctorReviewDto>();

        var deleteResp = await _client.DeleteAsync($"/api/v1/doctor-reviews/{created!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Create_ThenGetById_ReviewIsRetrievable()
    {
        var createResp = await _client.PostAsJsonAsync("/api/v1/doctor-reviews", ValidDto($"Dr. Lookup-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<DoctorReviewDto>();

        // Verify the created review is retrievable by its ID
        var getResp = await _client.GetAsync($"/api/v1/doctor-reviews/{created!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResp.Content.ReadFromJsonAsync<DoctorReviewDto>();
        fetched!.Id.Should().Be(created.Id);
        fetched.DoctorName.Should().Be(created.DoctorName);
    }
}
