namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using Application.Interfaces;

/// <summary>
/// Service for uploading and deleting item photos in the web root under /uploads/items.
/// Validates file presence, size (max 5MB), and allowed extensions (jpg, jpeg, png, webp);
/// saves files with GUID-based names and returns a relative URL. Uses IWebHostEnvironment
/// to resolve paths and safely deletes files by URL.
/// </summary>
public class PhotoService : IPhotoService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public PhotoService(IWebHostEnvironment env)
    {
        this._env = env;
    }

    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 5MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException("File type not allowed. Use jpg, png, or webp.");

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/items/{fileName}";
    }

    public Task DeletePhotoAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        var filePath = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
