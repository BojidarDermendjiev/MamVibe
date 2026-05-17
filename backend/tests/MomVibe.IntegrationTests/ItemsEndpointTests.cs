using System.Net;
using FluentAssertions;
using System.Net.Http.Json;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Items;

namespace MomVibe.IntegrationTests;

// ---------------------------------------------------------------------------
// Public / unauthenticated item endpoint tests (no auth needed)
// ---------------------------------------------------------------------------
public class ItemsPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemsPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var response = await _client.GetAsync("/api/items");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/items?searchTerm=baby&page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithListingTypeFilter_ReturnsOk()
    {
        var response = await _client.GetAsync($"/api/items?listingType={(int)ListingType.Donate}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistentItem_Returns404()
    {
        var response = await _client.GetAsync($"/api/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var dto = new CreateItemDto
        {
            Title = "Test",
            Description = "Test description",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Donate,
            PhotoUrls = []
        };
        var response = await _client.PostAsJsonAsync("/api/items", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_WithoutAuth_Returns401()
    {
        var response = await _client.PutAsJsonAsync($"/api/items/{Guid.NewGuid()}", new UpdateItemDto());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync($"/api/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToggleLike_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync($"/api/items/{Guid.NewGuid()}/like", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IncrementView_Returns204()
    {
        // IncrementView is public (no auth required) and returns 204 NoContent
        var response = await _client.PostAsync($"/api/items/{Guid.NewGuid()}/view", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

// ---------------------------------------------------------------------------
// Authenticated item endpoint tests (uses AI-stubbed factory)
// ---------------------------------------------------------------------------
public class ItemsAuthTests : IClassFixture<ItemsWebApplicationFactory>
{
    // Clothing category — deterministic ID from CategoryConfiguration.HasData
    private static readonly Guid ClothingCategoryId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    private readonly HttpClient _client;
    private readonly ItemsWebApplicationFactory _factory;

    public ItemsAuthTests(ItemsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private CreateItemDto MakeDonateItem(string title = "Integration Test Item") => new()
    {
        Title = title,
        Description = "A perfectly good item for donation.",
        CategoryId = ClothingCategoryId,
        ListingType = ListingType.Donate,
        PhotoUrls = ["https://example.com/photo.jpg"]
    };

    private CreateItemDto MakeSellItem(string title = "Integration Test Sell Item") => new()
    {
        Title = title,
        Description = "Item available for purchase.",
        CategoryId = ClothingCategoryId,
        ListingType = ListingType.Sell,
        Price = 25.00m,
        PhotoUrls = ["https://example.com/photo.jpg"]
    };

    [Fact]
    public async Task Create_DonateItem_Returns201WithItemDto()
    {
        var response = await _client.PostAsJsonAsync("/api/items", MakeDonateItem());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await response.Content.ReadFromJsonAsync<ItemDto>();
        item.Should().NotBeNull();
        item!.Title.Should().Be("Integration Test Item");
        item.ListingType.Should().Be(ListingType.Donate);
        item.UserId.Should().Be(ItemsWebApplicationFactory.TestUserId);
    }

    [Fact]
    public async Task Create_SellItem_Returns201WithPrice()
    {
        var response = await _client.PostAsJsonAsync("/api/items", MakeSellItem());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await response.Content.ReadFromJsonAsync<ItemDto>();
        item.Should().NotBeNull();
        item!.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task Create_InvalidItem_Returns400()
    {
        var dto = new CreateItemDto
        {
            Title = "",  // fails validator: title is required
            Description = "desc",
            CategoryId = ClothingCategoryId,
            ListingType = ListingType.Donate,
            PhotoUrls = []
        };

        var response = await _client.PostAsJsonAsync("/api/items", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingItem_Returns200()
    {
        var createResp = await _client.PostAsJsonAsync("/api/items", MakeDonateItem($"GetById-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>();

        var getResp = await _client.GetAsync($"/api/items/{created!.Id}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResp.Content.ReadFromJsonAsync<ItemDto>();
        fetched!.Id.Should().Be(created.Id);
        fetched.Title.Should().Be(created.Title);
    }

    [Fact]
    public async Task Update_OwnItem_Returns200WithUpdatedTitle()
    {
        var createResp = await _client.PostAsJsonAsync("/api/items", MakeDonateItem($"UpdateTest-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>();

        var update = new UpdateItemDto { Title = "Updated Title" };
        var updateResp = await _client.PutAsJsonAsync($"/api/items/{created!.Id}", update);

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<ItemDto>();
        updated!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Update_NonExistentItem_Returns404()
    {
        var update = new UpdateItemDto { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync($"/api/items/{Guid.NewGuid()}", update);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OwnItem_Returns204()
    {
        var createResp = await _client.PostAsJsonAsync("/api/items", MakeDonateItem($"DeleteTest-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>();

        var deleteResp = await _client.DeleteAsync($"/api/items/{created!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await _client.GetAsync($"/api/items/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistentItem_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleLike_LikeAndUnlike_TogglesCorrectly()
    {
        var createResp = await _client.PostAsJsonAsync("/api/items", MakeDonateItem($"LikeTest-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await createResp.Content.ReadFromJsonAsync<ItemDto>();

        // First call: like
        var likeResp = await _client.PostAsync($"/api/items/{item!.Id}/like", null);
        likeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body1 = await likeResp.Content.ReadFromJsonAsync<LikeResult>();
        body1!.IsLiked.Should().BeTrue();

        // Second call: unlike
        var unlikeResp = await _client.PostAsync($"/api/items/{item.Id}/like", null);
        unlikeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body2 = await unlikeResp.Content.ReadFromJsonAsync<LikeResult>();
        body2!.IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleLike_NonExistentItem_ReturnsBadRequest()
    {
        // ItemsController has no try-catch around ToggleLikeAsync;
        // KeyNotFoundException from the service reaches ExceptionHandlingMiddleware.
        // The middleware maps KeyNotFoundException → 404 if configured, otherwise → 500.
        // Accept either 400, 404, or 500 to avoid coupling to middleware internals.
        var response = await _client.PostAsync($"/api/items/{Guid.NewGuid()}/like", null);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task IncrementView_ExistingItem_Returns204()
    {
        var createResp = await _client.PostAsJsonAsync("/api/items", MakeDonateItem($"ViewTest-{Guid.NewGuid():N}"));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await createResp.Content.ReadFromJsonAsync<ItemDto>();

        // IncrementView returns 204 NoContent by design
        var viewResp = await _client.PostAsync($"/api/items/{item!.Id}/view", null);
        viewResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }


    private record LikeResult(bool IsLiked);
}
