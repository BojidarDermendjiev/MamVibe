namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Application.Interfaces;

/// <summary>
/// Authenticated API controller for photo management:
/// - Upload a single photo and receive its URL
/// - Delete a photo by its URL
/// All endpoints require authentication via the controller-level <c>[Authorize]</c> attribute.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("upload")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotosController"/>.
    /// </summary>
    /// <param name="photoService">Service handling photo storage operations.</param>
    public PhotosController(IPhotoService photoService)
    {
        this._photoService = photoService;
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
    /// </summary>
    /// <param name="url">The URL of the photo to delete.</param>
    /// <returns>
    /// 204 No Content on successful deletion.
    /// </returns>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string url)
    {
        await this._photoService.DeletePhotoAsync(url);
        return NoContent();
    }
}
