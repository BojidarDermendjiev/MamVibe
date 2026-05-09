namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.ChildFriendlyPlaces;
using Domain.Enums;

[ApiController]
[Route("api/child-friendly-places")]
public class ChildFriendlyPlacesController : ControllerBase
{
    private readonly IChildFriendlyPlaceService _service;
    private readonly ICurrentUserService _currentUser;

    public ChildFriendlyPlacesController(IChildFriendlyPlaceService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retrieves a paginated list of child-friendly places with optional filtering.
    /// </summary>
    /// <param name="city">Optional city name to filter results.</param>
    /// <param name="placeType">Optional place type to filter results.</param>
    /// <param name="childAgeMonths">Optional child age in months to filter age-appropriate places.</param>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of results per page, clamped to 1–50 (default: 20).</param>
    /// <returns>
    /// 200 OK with a paged result set of child-friendly places.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? city = null,
        [FromQuery] PlaceType? placeType = null,
        [FromQuery] int? childAgeMonths = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);
        var result = await _service.GetAllAsync(city, placeType, childAgeMonths, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single child-friendly place by its GUID.
    /// </summary>
    /// <param name="id">The GUID of the place to fetch.</param>
    /// <returns>
    /// 404 Not Found if the place does not exist.<br/>
    /// 200 OK with the place details on success.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var place = await _service.GetByIdAsync(id);
        return place == null ? NotFound() : Ok(place);
    }

    /// <summary>
    /// Creates a new child-friendly place submitted by the authenticated user.
    /// The place is queued for admin approval before becoming publicly visible.
    /// </summary>
    /// <param name="dto">Place creation payload including name, address, city, type, and age range.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 201 Created with the created place resource and location header.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChildFriendlyPlaceDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var place = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = place.Id }, place);
    }

    /// <summary>
    /// Deletes a child-friendly place. Only the submitting user or an administrator may delete.
    /// </summary>
    /// <param name="id">The GUID of the place to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the user is not permitted to delete the place.<br/>
    /// 404 Not Found if the place does not exist.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await _service.DeleteAsync(id, userId, isAdmin: _currentUser.IsAdmin);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
