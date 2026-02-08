namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Photo service contract:
/// - UploadPhotoAsync: accepts an IFormFile, stores it, and returns a relative URL.
/// - DeletePhotoAsync: deletes a previously uploaded photo by its URL.
/// </summary>
public interface IPhotoService
{
    Task<string> UploadPhotoAsync(IFormFile file);
    Task DeletePhotoAsync(string url);
}
