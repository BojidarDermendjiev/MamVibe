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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var place = await _service.GetByIdAsync(id);
        return place == null ? NotFound() : Ok(place);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChildFriendlyPlaceDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var place = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = place.Id }, place);
    }

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
