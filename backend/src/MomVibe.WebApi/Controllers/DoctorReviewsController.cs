namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.DoctorReviews;

[ApiController]
[Route("api/doctor-reviews")]
public class DoctorReviewsController : ControllerBase
{
    private readonly IDoctorReviewService _service;
    private readonly ICurrentUserService _currentUser;

    public DoctorReviewsController(IDoctorReviewService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? city = null,
        [FromQuery] string? specialization = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);
        var result = await _service.GetAllAsync(city, specialization, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var review = await _service.GetByIdAsync(id);
        return review == null ? NotFound() : Ok(review);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var reviews = await _service.GetByUserAsync(userId);
        return Ok(reviews);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorReviewDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var review = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
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
