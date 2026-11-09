using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Xunit;

namespace GymBase.Poc.Media.Tests.Integration;

public class MediaUploadFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly InMemoryMediaStorage _inMemoryStorage = new();

    public MediaUploadFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                // Thay thế IMediaStorage bằng InMemoryStorage để test tích hợp không phụ thuộc AWS/MinIO
                services.AddSingleton<IMediaStorage>(_inMemoryStorage);
                services.PostConfigure<MediaOptions>(opt =>
                {
                    opt.UseInMemoryStorage = true;
                    opt.AllowAnonymousForPoc = true;
                });
            });
        });
    }

    private static byte[] CreateTestJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.SteelBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data.ToArray();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Healthy");
    }

    [Fact]
    public async Task FullUploadFlow_RequestUrl_Put_Confirm_ShouldSucceed()
    {
        var client = _factory.CreateClient();
        var imageBytes = CreateTestJpeg(1200, 800);

        // BƯỚC 1: Xin cấp Presigned Upload URL
        var urlRequest = new UploadUrlRequest
        {
            Purpose = MediaPurpose.Avatar,
            ContentType = "image/jpeg",
            SizeBytes = imageBytes.Length
        };

        var urlResponse = await client.PostAsJsonAsync("/api/v1/media/upload-url", urlRequest);
        urlResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var urlData = await urlResponse.Content.ReadFromJsonAsync<UploadUrlResponse>();
        urlData.Should().NotBeNull();
        urlData!.PresignedPutUrl.Should().NotBeNullOrWhiteSpace();
        urlData.StagingKey.Should().StartWith("staging/avatar/");

        // BƯỚC 2: Giả lập Mobile PUT trực tiếp lên storage (sử dụng in-memory storage)
        _inMemoryStorage.DirectPut(urlData.StagingKey, imageBytes, "image/jpeg");

        // BƯỚC 3: Xác nhận hoàn tất upload
        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = urlData.UploadId,
            StagingKey = urlData.StagingKey,
            Purpose = MediaPurpose.Avatar
        };

        var confirmResponse = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmData = await confirmResponse.Content.ReadFromJsonAsync<ConfirmUploadResponse>();
        confirmData.Should().NotBeNull();
        confirmData!.MediaId.Should().NotBeNullOrWhiteSpace();
        confirmData.Urls.Thumb.Should().NotBeNullOrWhiteSpace();
        confirmData.Urls.Medium.Should().NotBeNullOrWhiteSpace();
        confirmData.Urls.Large.Should().NotBeNullOrWhiteSpace();
        confirmData.Width.Should().Be(1200);
        confirmData.Height.Should().Be(800);
        confirmData.Blurhash.Should().NotBeNullOrWhiteSpace();
        confirmData.IsDeduplicated.Should().BeFalse();

        // Kiểm tra staging đã được xóa sạch
        (await _inMemoryStorage.ExistsAsync(urlData.StagingKey)).Should().BeFalse();
    }

    [Fact]
    public async Task Confirm_BeforePut_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();

        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = "upload-not-put",
            StagingKey = "staging/avatar/2026/11/not-found-file.tmp",
            Purpose = MediaPurpose.Avatar
        };

        var response = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Object tạm không tồn tại");
    }

    [Fact]
    public async Task Confirm_Twice_ShouldBeIdempotent_AndSucceedBothTimes()
    {
        var client = _factory.CreateClient();
        var imageBytes = CreateTestJpeg(640, 480);

        // 1. Upload file vào staging
        const string stagingKey = "staging/gym_photo/2026/11/idempotent-test.tmp";
        _inMemoryStorage.DirectPut(stagingKey, imageBytes, "image/jpeg");

        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = "upload-idempotent",
            StagingKey = stagingKey,
            Purpose = MediaPurpose.GymPhoto
        };

        // Lần gọi 1
        var res1 = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);
        var data1 = await res1.Content.ReadFromJsonAsync<ConfirmUploadResponse>();

        // Giả lập Mobile mạng chập chờn gửi lại confirm lần 2 với cùng stagingKey (file đã bị xóa ở staging nhưng đã lưu asset)
        // Trường hợp staging đã chuyển thành asset hoặc client gọi lại với file mới có cùng content:
        _inMemoryStorage.DirectPut(stagingKey, imageBytes, "image/jpeg");
        var res2 = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
        var data2 = await res2.Content.ReadFromJsonAsync<ConfirmUploadResponse>();

        data2!.MediaId.Should().Be(data1!.MediaId);
        data2.ContentHash.Should().Be(data1.ContentHash);
    }
}
