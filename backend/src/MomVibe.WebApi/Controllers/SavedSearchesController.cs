namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.SavedSearches;

[ApiController]
[Route("api/saved-searches")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly ISavedSearchService _service;
    private readonly ICurrentUserService _currentUserService;

    public SavedSearchesController(ISavedSearchService service, ICurrentUserService currentUserService)
    {
        this._service = service;
        this._currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMy()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._service.GetMyAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSavedSearchDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            var result = await this._service.CreateAsync(userId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._service.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
