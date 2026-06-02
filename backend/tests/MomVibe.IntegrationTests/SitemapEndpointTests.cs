using System.Net;
using FluentAssertions;

namespace MomVibe.IntegrationTests;

public class SitemapEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SitemapEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItemsSitemap_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/sitemap/items");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetItemsSitemap_ReturnsXmlContentType()
    {
        var response = await _client.GetAsync("/api/v1/sitemap/items");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/xml");
    }

    [Fact]
    public async Task GetItemsSitemap_ReturnsWellFormedXml()
    {
        var response = await _client.GetAsync("/api/v1/sitemap/items");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<?xml");
        content.Should().Contain("<urlset");
        content.Should().Contain("</urlset>");
    }

    [Fact]
    public async Task GetItemsSitemap_ContainsUrlsetElement()
    {
        var response = await _client.GetAsync("/api/v1/sitemap/items");
        var content = await response.Content.ReadAsStringAsync();

        // Verify the response is a proper sitemap XML with a urlset root element
        content.Should().Contain("xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
    }
}
