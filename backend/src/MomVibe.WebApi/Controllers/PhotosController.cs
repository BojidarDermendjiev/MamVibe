namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;

/// <summary>
/// Authenticated API controller for photo management:
/// - Upload a single photo and receive its URL
/// - Delete a photo by its URL (ownership verified against the requesting user)
/// All endpoints require authentication via the controller-level <c>[Authorize]</c> attribute.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.Upload)]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotosController"/>.
    /// </summary>
    /// <param name="photoService">Service handling photo storage operations.</param>
    /// <param name="currentUserService">Service providing the current authenticated user context.</param>
    public PhotosController(IPhotoService photoService, ICurrentUserService currentUserService)
    {
        this._photoService = photoService;
        this._currentUserService = currentUserService;
    }

    /// <summary>
    /// Uploads a single photo file and returns its accessible URL.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>
    /// 400 Bad Request if no file is provided or validation fails (with error message).<br/>
    /// 200 OK with a JSON payload containing the uploaded photo URL: <c>{ url }</c>.
    /// </returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        try
        {
            var url = await this._photoService.UploadPhotoAsync(file);
            return Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a photo by its URL.
    /// Only succeeds when the photo belongs to an item owned by the requesting user.
    /// </summary>
    /// <param name="url">The URL of the photo to delete.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 403 Forbidden if the photo does not belong to the requesting user.<br/>
    /// 204 No Content on successful deletion.
    /// </returns>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string url)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var deleted = await this._photoService.DeletePhotoWithOwnershipCheckAsync(url, userId);
        if (!deleted) return StatusCode(403, new { message = "You do not have permission to delete this photo." });

        return NoContent();
    }
}
