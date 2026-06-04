namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;

/// <summary>
/// Public endpoint that exposes aggregate platform statistics displayed on the home page.
/// No authentication required; data is read-only and intentionally non-sensitive.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
public class StatsController : ControllerBase
{
    private readonly IItemService _itemService;

    public StatsController(IItemService itemService)
    {
        this._itemService = itemService;
    }

    /// <summary>
    /// Returns headline platform statistics: active listing count, unique seller count,
    /// and the number of families who have completed at least one transaction.
    /// Intended for the public-facing landing page; no authentication required.
    /// </summary>
    /// <returns>
    /// 200 OK with <c>{ activeListings, totalSellers, happyFamilies }</c>.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetPublicStats()
    {
        var stats = await this._itemService.GetPublicStatsAsync();
        return Ok(stats);
    }
}
