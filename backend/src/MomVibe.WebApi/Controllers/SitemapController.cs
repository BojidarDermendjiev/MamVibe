namespace MomVibe.WebApi.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Items;
using Application.Interfaces;

/// <summary>
/// Serves a dynamic XML sitemap that extends the static /sitemap.xml with
/// per-item URLs so Google can crawl individual product pages without
/// requiring manual sitemap maintenance.
///
/// SEO rationale:
/// - Item detail pages (/items/:id) are the primary commercial landing pages.
///   Each one has unique title, description, and Product structured data.
///   Including them in the sitemap ensures Google discovers and indexes them.
/// - We only include active, non-reserved items (isActive = true) to avoid
///   surfacing sold/donated items that return 404-equivalent states.
/// - lastmod is set to the item's creation date as a reasonable proxy.
/// - Priority 0.7 reflects these as high-value but secondary to the homepage
///   (1.0) and browse page (0.9) in the static sitemap.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SitemapController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<SitemapController> _logger;

    /// <summary>Initialises the sitemap controller with required services.</summary>
    public SitemapController(IItemService itemService, ILogger<SitemapController> logger)
    {
        this._itemService = itemService;
        this._logger = logger;
    }

    /// <summary>
    /// Returns an XML sitemap containing all active item listing URLs.
    /// Intended to be fetched periodically by a build script and merged with
    /// the static /sitemap.xml, or referenced directly in robots.txt as a
    /// supplementary sitemap index entry.
    /// </summary>
    /// <returns>application/xml sitemap with one &lt;url&gt; per active item.</returns>
    [HttpGet("items")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetItemsSitemap()
    {
        try
        {
            // Fetch all active items in a single large page.
            // For very large catalogues (>10 000 items), split into
            // sitemap index files — each sitemap must be ≤ 50 000 URLs.
            var filter = new ItemFilterDto
            {
                Page = 1,
                PageSize = 10_000,
                SortBy = "newest",
            };

            var pagedItems = await this._itemService.GetAllAsync(filter);

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            foreach (var item in pagedItems.Items)
            {
                var escapedTitle = System.Security.SecurityElement.Escape(item.Title);
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>https://mamvibe.com/items/{item.Id}</loc>");
                sb.AppendLine($"    <lastmod>{item.CreatedAt:yyyy-MM-dd}</lastmod>");
                sb.AppendLine("    <changefreq>weekly</changefreq>");
                sb.AppendLine("    <priority>0.7</priority>");
                sb.AppendLine("  </url>");
            }

            sb.AppendLine("</urlset>");

            return this.Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to generate items sitemap");
            return this.StatusCode(500, "Sitemap generation failed");
        }
    }
}
