namespace MomVibe.Infrastructure.Services;

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Configuration;

/// <summary>
/// Cloudflare R2 implementation of <see cref="IPhotoService"/>.
/// Uses the S3-compatible R2 API to store and delete photos.
/// All validation is delegated to <see cref="PhotoHelper"/>.
/// Activated when R2:AccountId, R2:AccessKeyId, R2:SecretAccessKey, R2:BucketName,
/// and R2:PublicUrl are all set in configuration.
/// </summary>
public class R2PhotoService : IPhotoService
{
    private readonly AmazonS3Client _s3;
    private readonly R2Settings _settings;
    private readonly IApplicationDbContext _context;

    public R2PhotoService(IOptions<R2Settings> settings, IApplicationDbContext context)
    {
        _settings = settings.Value;
        _context = context;

        _s3 = new AmazonS3Client(
            _settings.AccessKeyId,
            _settings.SecretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = $"https://{_settings.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            });
    }

    /// <inheritdoc/>
    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        var (stream, extension) = await PhotoHelper.ValidateAndProcessAsync(file);
        using var processedStream = stream;

        var key = $"items/{Guid.NewGuid()}{extension}";

        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = processedStream,
            ContentType = file.ContentType,
            DisablePayloadSigning = true
        });

        return $"{_settings.PublicUrl.TrimEnd('/')}/{key}";
    }

    /// <inheritdoc/>
    public Task DeletePhotoAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        var prefix = _settings.PublicUrl.TrimEnd('/') + "/";
        if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var key = url[prefix.Length..];
        if (string.IsNullOrEmpty(key) || key.Contains(".."))
            return Task.CompletedTask;

        return _s3.DeleteObjectAsync(_settings.BucketName, key);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePhotoWithOwnershipCheckAsync(string url, string userId)
    {
        if (string.IsNullOrEmpty(url)) return false;

        var ownedPhoto = await _context.ItemPhotos
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Url == url && p.Item.UserId == userId);

        if (ownedPhoto == null) return false;

        await DeletePhotoAsync(url);
        return true;
    }
}
