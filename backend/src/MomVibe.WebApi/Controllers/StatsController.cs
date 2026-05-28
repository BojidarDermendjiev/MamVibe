namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class StatsController : ControllerBase
{
    private readonly IItemService _itemService;

    public StatsController(IItemService itemService)
    {
        this._itemService = itemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicStats()
    {
        var stats = await this._itemService.GetPublicStatsAsync();
        return Ok(stats);
    }
}
