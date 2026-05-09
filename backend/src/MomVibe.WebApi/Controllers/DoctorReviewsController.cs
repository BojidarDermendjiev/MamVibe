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

    /// <summary>
    /// Retrieves a paginated list of approved doctor reviews with optional filtering.
    /// </summary>
    /// <param name="city">Optional city name to filter reviews.</param>
    /// <param name="specialization">Optional medical specialization to filter reviews.</param>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of results per page, clamped to 1–50 (default: 20).</param>
    /// <returns>
    /// 200 OK with a paged result set of doctor reviews.
    /// </returns>
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

    /// <summary>
    /// Retrieves a single doctor review by its GUID.
    /// </summary>
    /// <param name="id">The GUID of the review to fetch.</param>
    /// <returns>
    /// 404 Not Found if the review does not exist.<br/>
    /// 200 OK with the review details on success.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var review = await _service.GetByIdAsync(id);
        return review == null ? NotFound() : Ok(review);
    }

    /// <summary>
    /// Retrieves all doctor reviews submitted by the authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 200 OK with the user's submitted reviews on success.
    /// </returns>
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var reviews = await _service.GetByUserAsync(userId);
        return Ok(reviews);
    }

    /// <summary>
    /// Submits a new doctor review from the authenticated user.
    /// The review is queued for admin approval before becoming publicly visible.
    /// </summary>
    /// <param name="dto">Review payload including doctor name, city, specialization, rating, and comment.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 201 Created with the created review resource and location header.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorReviewDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();
        var review = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
    }

    /// <summary>
    /// Deletes a doctor review. Only the submitting user or an administrator may delete.
    /// </summary>
    /// <param name="id">The GUID of the review to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the user is not permitted to delete the review.<br/>
    /// 404 Not Found if the review does not exist.<br/>
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
