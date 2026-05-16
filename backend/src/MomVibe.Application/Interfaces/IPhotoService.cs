namespace MomVibe.Application.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Photo service contract:
/// - UploadPhotoAsync: accepts an IFormFile, stores it, and returns a relative URL.
/// - DeletePhotoAsync: deletes a previously uploaded photo by its URL.
/// - DeletePhotoWithOwnershipCheckAsync: deletes a photo only if it belongs to an item owned by the requesting user.
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

    /// <summary>
    /// Deletes a photo only when the <c>ItemPhoto</c> record with that URL belongs to an item
    /// owned by <paramref name="userId"/>. Returns <c>false</c> if no matching owned record is
    /// found (caller should respond with 403); returns <c>true</c> on successful deletion.
    /// </summary>
    /// <param name="url">The relative URL of the photo to delete.</param>
    /// <param name="userId">The identifier of the user requesting the deletion.</param>
    Task<bool> DeletePhotoWithOwnershipCheckAsync(string url, string userId);
}
