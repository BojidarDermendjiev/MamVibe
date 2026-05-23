namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using Application.Interfaces;

/// <summary>
/// Local file-system implementation of <see cref="IPhotoService"/>.
/// Stores images in <c>wwwroot/uploads/items/</c>. Used in development and as a fallback
/// when Cloudflare R2 is not configured. Validation is delegated to <see cref="PhotoHelper"/>.
/// </summary>
public class PhotoService : IPhotoService
{
    private readonly IWebHostEnvironment _env;
    private readonly IApplicationDbContext _context;

    public PhotoService(IWebHostEnvironment env, IApplicationDbContext context)
    {
        this._env = env;
        this._context = context;
    }

    /// <inheritdoc/>
    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        var (stream, extension) = await PhotoHelper.ValidateAndProcessAsync(file);
        using var processedStream = stream;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await processedStream.CopyToAsync(fileStream);

        return $"/uploads/items/{fileName}";
    }

    /// <inheritdoc/>
    public Task DeletePhotoAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        // Prevent path traversal: only allow files in /uploads/items/ with GUID names
        var fileName = Path.GetFileName(url);
        if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
            return Task.CompletedTask;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
        var filePath = Path.Combine(uploadsDir, fileName);

        // Verify the resolved path is still within uploads directory
        var fullPath = Path.GetFullPath(filePath);
        var fullUploadsDir = Path.GetFullPath(uploadsDir);
        if (!fullPath.StartsWith(fullUploadsDir, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePhotoWithOwnershipCheckAsync(string url, string userId)
    {
        if (string.IsNullOrEmpty(url)) return false;

        // Verify the photo record exists and belongs to an item owned by the requesting user.
        // The JOIN through Item ensures no cross-user deletions are possible.
        var ownedPhoto = await this._context.ItemPhotos
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Url == url && p.Item.UserId == userId);

        if (ownedPhoto == null)
            return false;

        await DeletePhotoAsync(url);
        return true;
    }
}
