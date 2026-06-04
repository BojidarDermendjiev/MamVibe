namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.SavedSearches;

/// <summary>
/// Authenticated endpoints for managing saved item searches.
/// A saved search stores a user's active filter criteria so they can be re-applied
/// and, optionally, trigger push notifications when new matching items are listed.
/// All endpoints require authentication.
/// </summary>
[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/saved-searches")]
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

    /// <summary>
    /// Returns all saved searches belonging to the authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of the caller's saved searches, ordered by creation date descending.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetMy()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var result = await this._service.GetMyAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new saved search for the authenticated user.
    /// The search criteria (category, listing type, age group, price range, etc.)
    /// are captured from the DTO and persisted for later re-use or notification matching.
    /// </summary>
    /// <param name="dto">The search filter criteria to save.</param>
    /// <returns>
    /// 200 OK with the newly created saved search.<br/>
    /// 400 Bad Request if the user has reached the saved-search limit or the criteria are invalid.<br/>
    /// 401 Unauthorized if the caller is not authenticated.
    /// </returns>
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

    /// <summary>
    /// Deletes a saved search owned by the authenticated user.
    /// </summary>
    /// <param name="id">The GUID of the saved search to delete.</param>
    /// <returns>
    /// 204 No Content on success.<br/>
    /// 401 Unauthorized if the caller is not authenticated.<br/>
    /// 403 Forbidden if the caller does not own the saved search.<br/>
    /// 404 Not Found if no saved search with the given ID exists.
    /// </returns>
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
