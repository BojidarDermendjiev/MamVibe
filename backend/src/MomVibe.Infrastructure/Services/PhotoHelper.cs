namespace MomVibe.Infrastructure.Services;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Shared photo validation and processing logic used by both PhotoService (local disk)
/// and R2PhotoService (Cloudflare R2). Validates size, extension, content-type, magic bytes,
/// image dimensions, and strips EXIF/GPS metadata from JPEG files.
/// </summary>
internal static class PhotoHelper
{
    internal static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    internal const long MaxFileSize = 5 * 1024 * 1024;
    private const int MaxImageDimension = 8000;

    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        { ".jpg",  [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".jpeg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".png",  [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }] },
        { ".webp", [new byte[] { 0x52, 0x49, 0x46, 0x46 }] }
    };

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    /// <summary>
    /// Validates the uploaded file and returns the processed stream (EXIF stripped for JPEG)
    /// and its extension. The caller owns the returned stream and must dispose it.
    /// </summary>
    internal static async Task<(MemoryStream Stream, string Extension)> ValidateAndProcessAsync(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 5MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("File type not allowed. Use jpg, png, or webp.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Invalid content type. Only image files are allowed.");

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        // Magic byte validation
        var header = new byte[8];
        await ms.ReadAsync(header.AsMemory(0, Math.Min(8, (int)ms.Length)));
        if (FileSignatures.TryGetValue(extension, out var signatures))
        {
            var isValid = signatures.Any(sig =>
                header.Length >= sig.Length &&
                header.Take(sig.Length).SequenceEqual(sig));
            if (!isValid)
            {
                ms.Dispose();
                throw new ArgumentException("File content does not match its extension.");
            }
        }

        ms.Position = 0;
        ValidateImageDimensions(extension, ms);

        // Strip EXIF/GPS from JPEG to protect user privacy
        if (extension is ".jpg" or ".jpeg")
        {
            ms.Position = 0;
            var stripped = StripJpegMetadata(ms);
            ms.Dispose();
            return (stripped, extension);
        }

        ms.Position = 0;
        return (ms, extension);
    }

    private static void ValidateImageDimensions(string extension, MemoryStream stream)
    {
        try
        {
            if (extension is ".png")
            {
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
        catch (Exception ex)
        {
            throw new ArgumentException($"Unable to validate image dimensions: {ex.Message}", ex);
        }
    }

    private static MemoryStream StripJpegMetadata(MemoryStream source)
    {
        var data = source.ToArray();
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
            return source;

        var output = new MemoryStream(data.Length);
        output.WriteByte(0xFF);
        output.WriteByte(0xD8);
        var pos = 2;

        while (pos < data.Length - 1)
        {
            if (data[pos] != 0xFF) break;
            var marker = data[pos + 1];
            pos += 2;

            if (marker == 0xDA)
            {
                output.WriteByte(0xFF);
                output.WriteByte(marker);
                output.Write(data, pos, data.Length - pos);
                break;
            }

            if (marker == 0xD9)
            {
                output.WriteByte(0xFF);
                output.WriteByte(marker);
                break;
            }

            if (marker >= 0xD0 && marker <= 0xD8)
            {
                output.WriteByte(0xFF);
                output.WriteByte(marker);
                continue;
            }

            if (pos + 1 >= data.Length) break;
            var segmentLength = (data[pos] << 8) | data[pos + 1];
            if (segmentLength < 2 || pos + segmentLength > data.Length) break;

            // APP1 = EXIF/XMP, APP13 = IPTC/Photoshop — drop both
            if (marker == 0xE1 || marker == 0xED)
            {
                pos += segmentLength;
                continue;
            }

            output.WriteByte(0xFF);
            output.WriteByte(marker);
            output.Write(data, pos, segmentLength);
            pos += segmentLength;
        }

        output.Position = 0;
        return output;
    }
}
