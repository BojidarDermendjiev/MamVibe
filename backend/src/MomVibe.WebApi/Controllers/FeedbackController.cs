namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Feedbacks;

/// <summary>
/// Public and authenticated endpoints for managing user feedback:
/// - List feedback (paginated)
/// - Create feedback (authenticated)
/// - Delete feedback (authenticated; author or admin)
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackController"/>.
    /// </summary>
    /// <param name="feedbackService">Service for feedback operations.</param>
    /// <param name="currentUserService">Service providing current user context.</param>
    public FeedbackController(IFeedbackService feedbackService, ICurrentUserService currentUserService)
    {
        this._feedbackService = feedbackService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of feedback entries.
    /// </summary>
    /// <param name="page">1-based page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <returns>
    /// 200 OK with a paged result set of feedback entries.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await this._feedbackService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new feedback entry for the authenticated user.
    /// </summary>
    /// <param name="dto">Feedback details including rating, category, content, and contact preference.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 201 Created with the created feedback resource.
    /// </returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var feedback = await this._feedbackService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetAll), feedback);
    }

    /// <summary>
    /// Deletes a feedback entry. Only the author or an administrator may delete.
    /// </summary>
    /// <param name="id">The GUID of the feedback to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbid if the user is not permitted to delete the feedback.<br/>
    /// 404 Not Found if the feedback does not exist.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._feedbackService.DeleteAsync(id, userId, this._currentUserService.IsAdmin);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
