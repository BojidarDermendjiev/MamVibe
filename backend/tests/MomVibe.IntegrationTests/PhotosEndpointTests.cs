using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using MomVibe.Application.Interfaces;

namespace MomVibe.IntegrationTests;

// ── Photo service stubs ─────────────────────────────────────────────────────

file class PermittedPhotoService : IPhotoService
{
    public Task<string> UploadPhotoAsync(IFormFile file) =>
        Task.FromResult($"/uploads/{file.FileName}");
    public Task DeletePhotoAsync(string url) => Task.CompletedTask;
    public Task<bool> DeletePhotoWithOwnershipCheckAsync(string url, string userId) =>
        Task.FromResult(true);
}

file class ForbiddenPhotoService : IPhotoService
{
    public Task<string> UploadPhotoAsync(IFormFile file) =>
        Task.FromResult("/uploads/test.jpg");
    public Task DeletePhotoAsync(string url) => Task.CompletedTask;
    public Task<bool> DeletePhotoWithOwnershipCheckAsync(string url, string userId) =>
        Task.FromResult(false);   // simulates "photo not owned by this user"
}

// ── Factories ───────────────────────────────────────────────────────────────

public class PhotosPermittedFactory : GeneralAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            var d = services.SingleOrDefault(s => s.ServiceType == typeof(IPhotoService));
            if (d != null) services.Remove(d);
            services.AddScoped<IPhotoService, PermittedPhotoService>();
        });
    }
}

public class PhotosForbiddenFactory : GeneralAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            var d = services.SingleOrDefault(s => s.ServiceType == typeof(IPhotoService));
            if (d != null) services.Remove(d);
            services.AddScoped<IPhotoService, ForbiddenPhotoService>();
        });
    }
}

// ── Public / unauthenticated tests ─────────────────────────────────────────

public class PhotosPublicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PhotosPublicTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_WithoutAuth_Returns401()
    {
        using var content = BuildFileContent("test.jpg");
        var response = await _client.PostAsync("/api/v1/photos/upload", content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync("/api/v1/photos?url=/uploads/test.jpg");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static MultipartFormDataContent BuildFileContent(string filename, string body = "fake image data")
    {
        var content = new MultipartFormDataContent();
        var fileBytes = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
        fileBytes.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileBytes, "file", filename);
        return content;
    }
}

// ── Authenticated tests — permitted service (upload succeeds, delete allowed) ──

public class PhotosAuthTests : IClassFixture<PhotosPermittedFactory>
{
    private readonly HttpClient _client;

    public PhotosAuthTests(PhotosPermittedFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_NoFilePart_Returns400()
    {
        // Send an empty multipart body — no "file" part
        using var content = new MultipartFormDataContent();
        var response = await _client.PostAsync("/api/v1/photos/upload", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_EmptyFile_Returns400()
    {
        using var content = new MultipartFormDataContent();
        var emptyFile = new ByteArrayContent([]);
        emptyFile.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(emptyFile, "file", "empty.jpg");

        var response = await _client.PostAsync("/api/v1/photos/upload", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_ValidFile_Returns200WithUrl()
    {
        using var content = BuildFileContent("baby-jacket.jpg");
        var response = await _client.PostAsync("/api/v1/photos/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        body!.Url.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Delete_OwnedPhoto_Returns204()
    {
        var response = await _client.DeleteAsync("/api/v1/photos?url=/uploads/my-photo.jpg");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static MultipartFormDataContent BuildFileContent(string filename, string body = "fake image bytes")
    {
        var content = new MultipartFormDataContent();
        var fileBytes = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
        fileBytes.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileBytes, "file", filename);
        return content;
    }

    private record UploadResponse(string Url);
}

// ── Authenticated tests — forbidden service (delete returns 403) ─────────────

public class PhotosForbiddenTests : IClassFixture<PhotosForbiddenFactory>
{
    private readonly HttpClient _client;

    public PhotosForbiddenTests(PhotosForbiddenFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Delete_NotOwnedPhoto_Returns403()
    {
        var response = await _client.DeleteAsync("/api/v1/photos?url=/uploads/someone-elses-photo.jpg");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
