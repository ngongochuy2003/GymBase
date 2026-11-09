using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using GymBase.Poc.Media.BL;
using GymBase.Poc.Media.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Xunit;

namespace GymBase.Poc.Media.Tests.Integration;

public class FilesApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _testTempDir;

    public FilesApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "gymbase_api_integration_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.PostConfigure<StorageOptions>(opt =>
                {
                    opt.DefaultProvider = "LOCAL";
                    opt.LocalRoot = _testTempDir;
                    opt.AllowAnonymousForPoc = true;
                });
            });
        });
    }

    [Fact]
    public async Task Upload_GetMetadata_GetContent_Delete_FullFlow_ShouldSucceed()
    {
        var client = _factory.CreateClient();

        // 1. Tải file JPEG lên qua POST /api/files (multipart/form-data)
        var jpegBytes = CreateTestJpeg(120, 120);
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(jpegBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "avatar.jpg");
        content.Add(new StringContent("avatar"), "purpose");

        var uploadResponse = await client.PostAsync("/api/files", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<FileUploadResultDto>();
        uploadResult.Should().NotBeNull();
        uploadResult!.OriginalName.Should().Be("avatar.jpg");
        uploadResult.ContentType.Should().Be("image/webp");
        uploadResult.Provider.Should().Be("LOCAL");
        uploadResult.Blurhash.Should().NotBeNullOrWhiteSpace();
        uploadResult.ContentUrl.Should().Be($"/api/files/{uploadResult.FileId}/content");

        // 2. Lấy metadata qua GET /api/files/{id}
        var metaResponse = await client.GetAsync($"/api/files/{uploadResult.FileId}");
        metaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metadata = await metaResponse.Content.ReadFromJsonAsync<TepTinMetadataDto>();
        metadata.Should().NotBeNull();
        metadata!.Id.Should().Be(uploadResult.FileId);
        metadata.Status.Should().Be("READY");

        // 3. Tải nội dung file qua GET /api/files/{id}/content
        var contentResponse = await client.GetAsync($"/api/files/{uploadResult.FileId}/content");
        contentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        contentResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
        var downloadedBytes = await contentResponse.Content.ReadAsByteArrayAsync();
        downloadedBytes.Length.Should().BeGreaterThan(0);

        // 4. Xóa file qua DELETE /api/files/{id}
        var deleteResponse = await client.DeleteAsync($"/api/files/{uploadResult.FileId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Kiểm tra GET content sau khi xóa -> 404
        var getContentAfterDelete = await client.GetAsync($"/api/files/{uploadResult.FileId}/content");
        getContentAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_WithFakeImage_DisguisedAsJpeg_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient();

        var fakeBytes = "Not an image, just random text content"u8.ToArray();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fakeBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "fake.jpg");
        content.Add(new StringContent("avatar"), "purpose");

        var response = await client.PostAsync("/api/files", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().ContainEquivalentOf("magic bytes");
    }

    [Fact]
    public async Task Upload_WithoutMultipart_ShouldReturnUnsupportedMediaType()
    {
        var client = _factory.CreateClient();
        var stringContent = new StringContent("{\"purpose\":\"avatar\"}");

        var response = await client.PostAsync("/api/files", stringContent);
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempDir))
        {
            try { Directory.Delete(_testTempDir, true); } catch { }
        }
    }

    private static byte[] CreateTestJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.ForestGreen);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data.ToArray();
    }
}
