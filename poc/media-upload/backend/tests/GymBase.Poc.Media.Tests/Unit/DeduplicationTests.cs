using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Processing;
using GymBase.Poc.Media.Services;
using GymBase.Poc.Media.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class DeduplicationTests
{
    private static byte[] CreateTestJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.DarkGreen);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data.ToArray();
    }

    [Fact]
    public async Task ConfirmUpload_WithIdenticalImageContent_ShouldDeduplicateAndReuseExistingAsset()
    {
        var inMemoryStorage = new InMemoryMediaStorage();
        var processor = new SkiaImageProcessor();
        var options = new MediaOptions { UseInMemoryStorage = true };

        var service = new MediaService(
            inMemoryStorage,
            processor,
            Options.Create(options),
            NullLogger<MediaService>.Instance);

        var imageBytes = CreateTestJpeg(800, 600);

        // 1. Upload lần đầu
        const string stagingKey1 = "staging/gym_photo/2026/11/upload-1.tmp";
        inMemoryStorage.DirectPut(stagingKey1, imageBytes, "image/jpeg");

        var response1 = await service.ConfirmUploadAsync(new ConfirmUploadRequest
        {
            UploadId = "upload-1",
            StagingKey = stagingKey1,
            Purpose = MediaPurpose.GymPhoto
        }, "user-1");

        response1.IsDeduplicated.Should().BeFalse();
        response1.MediaId.Should().NotBeNullOrWhiteSpace();
        (await inMemoryStorage.ExistsAsync(stagingKey1)).Should().BeFalse("Staging 1 phải bị xóa sau khi confirm");

        // 2. Upload lần hai với file hoàn toàn giống hệt
        const string stagingKey2 = "staging/gym_photo/2026/11/upload-2.tmp";
        inMemoryStorage.DirectPut(stagingKey2, imageBytes, "image/jpeg");

        var response2 = await service.ConfirmUploadAsync(new ConfirmUploadRequest
        {
            UploadId = "upload-2",
            StagingKey = stagingKey2,
            Purpose = MediaPurpose.GymPhoto
        }, "user-2");

        // 3. Kỳ vọng deduplication kích hoạt
        response2.IsDeduplicated.Should().BeTrue("Ảnh có cùng hash SHA-256 phải được deduplicate");
        response2.MediaId.Should().Be(response1.MediaId, "Phải tái sử dụng MediaId ban đầu");
        response2.ContentHash.Should().Be(response1.ContentHash);
        response2.Urls.Thumb.Should().Be(response1.Urls.Thumb);
        response2.Urls.Medium.Should().Be(response1.Urls.Medium);
        response2.Urls.Large.Should().Be(response1.Urls.Large);

        (await inMemoryStorage.ExistsAsync(stagingKey2)).Should().BeFalse("Staging 2 phải bị dọn dẹp sau deduplicate");
    }
}
