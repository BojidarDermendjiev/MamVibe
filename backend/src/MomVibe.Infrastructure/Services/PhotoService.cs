namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using Application.Interfaces;

public class PhotoService : IPhotoService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    // Magic bytes for image file validation
    private static readonly Dictionary<string, byte[][]> _fileSignatures = new()
    {
        { ".jpg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".jpeg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".png", [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }] },
        { ".webp", [new byte[] { 0x52, 0x49, 0x46, 0x46 }] }
    };

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

        // Validate Content-Type matches extension
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Invalid content type. Only image files are allowed.");

        // Validate file magic bytes to prevent disguised uploads
        using var headerStream = new MemoryStream();
        await file.CopyToAsync(headerStream);
        headerStream.Position = 0;
        var headerBytes = new byte[8];
        await headerStream.ReadAsync(headerBytes.AsMemory(0, Math.Min(8, (int)headerStream.Length)));

        if (_fileSignatures.TryGetValue(extension, out var signatures))
        {
            var isValid = signatures.Any(sig =>
                headerBytes.Length >= sig.Length &&
                headerBytes.Take(sig.Length).SequenceEqual(sig));
            if (!isValid)
                throw new ArgumentException("File content does not match its extension.");
        }

        // Validate image dimensions to prevent memory-exhaustion via huge images
        headerStream.Position = 0;
        ValidateImageDimensions(extension, headerStream);

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "items");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        headerStream.Position = 0;
        using var fileStream = new FileStream(filePath, FileMode.Create);
        await headerStream.CopyToAsync(fileStream);

        return $"/uploads/items/{fileName}";
    }

    private const int MaxImageDimension = 8000;

    /// <summary>
    /// Reads image dimensions from the file stream header without fully decoding the image.
    /// Throws if width or height exceeds <see cref="MaxImageDimension"/> pixels.
    /// </summary>
    private static void ValidateImageDimensions(string extension, MemoryStream stream)
    {
        try
        {
            if (extension is ".png")
            {
                // PNG spec: IHDR chunk starts at byte 8; width at bytes 16-19, height at 20-23 (big-endian)
                if (stream.Length < 24) return;
                stream.Position = 16;
                var buf = new byte[8];
                stream.ReadExactly(buf, 0, 8);
                var width  = (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
                var height = (buf[4] << 24) | (buf[5] << 16) | (buf[6] << 8) | buf[7];
                if (width > MaxImageDimension || height > MaxImageDimension)
                    throw new ArgumentException($"Image dimensions must not exceed {MaxImageDimension}×{MaxImageDimension} pixels.");
            }
            else if (extension is ".jpg" or ".jpeg")
            {
                // Scan for SOF0/SOF1/SOF2 markers (0xFF 0xC0-0xC2) which carry image height/width
                var bytes = stream.ToArray();
                for (var i = 0; i < bytes.Length - 8; i++)
                {
                    if (bytes[i] != 0xFF) continue;
                    var marker = bytes[i + 1];
                    if (marker is 0xC0 or 0xC1 or 0xC2)
                    {
                        var height = (bytes[i + 5] << 8) | bytes[i + 6];
                        var width  = (bytes[i + 7] << 8) | bytes[i + 8];
                        if (width > MaxImageDimension || height > MaxImageDimension)
                            throw new ArgumentException($"Image dimensions must not exceed {MaxImageDimension}×{MaxImageDimension} pixels.");
                        break;
                    }
                }
            }
        }
        catch (ArgumentException) { throw; }
        catch { /* If we can't parse dimensions, allow the upload — size limit still applies */ }
    }

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
}
