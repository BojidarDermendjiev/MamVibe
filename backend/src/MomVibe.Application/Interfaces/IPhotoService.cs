namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Photo service contract:
/// - UploadPhotoAsync: accepts an IFormFile, stores it, and returns a relative URL.
/// - DeletePhotoAsync: deletes a previously uploaded photo by its URL.
/// </summary>
public interface IPhotoService
{
    /// <summary>Accepts an uploaded file, stores it, and returns its relative URL.</summary>
    /// <param name="file">The uploaded file to store.</param>
    /// <returns>The relative URL of the stored photo.</returns>
    Task<string> UploadPhotoAsync(IFormFile file);

    /// <summary>Deletes a previously uploaded photo identified by its URL.</summary>
    /// <param name="url">The relative or absolute URL of the photo to delete.</param>
    Task DeletePhotoAsync(string url);
}
