using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Processing;
using GymBase.Poc.Media.Services;
using GymBase.Poc.Media.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class MediaValidatorTests
{
    private readonly MediaService _service;
    private readonly Mock<IMediaStorage> _storageMock;
    private readonly Mock<IImageProcessor> _processorMock;
    private readonly MediaOptions _options;

    public MediaValidatorTests()
    {
        _storageMock = new Mock<IMediaStorage>();
        _processorMock = new Mock<IImageProcessor>();
        _options = new MediaOptions();

        _service = new MediaService(
            _storageMock.Object,
            _processorMock.Object,
            Options.Create(_options),
            NullLogger<MediaService>.Instance);
    }

    [Theory]
    [InlineData("avatar")]
    [InlineData("gym_photo")]
    [InlineData("progress_photo")]
    [InlineData("post_image")]
    public async Task GetUploadUrl_WithValidPurpose_ShouldSucceed(string purpose)
    {
        _storageMock
            .Setup(s => s.GetPresignedPutUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), default))
            .ReturnsAsync("https://s3.mock/presigned-put");

        var request = new UploadUrlRequest
        {
            Purpose = purpose,
            ContentType = "image/jpeg",
            SizeBytes = 1024 * 500
        };

        var response = await _service.GetUploadUrlAsync(request, "user-123");

        response.Should().NotBeNull();
        response.PresignedPutUrl.Should().Be("https://s3.mock/presigned-put");
        response.StagingKey.Should().StartWith($"staging/{purpose}/");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("random_hack")]
    [InlineData("admin_banner")]
    public async Task GetUploadUrl_WithInvalidPurpose_ShouldThrowArgumentException(string invalidPurpose)
    {
        var request = new UploadUrlRequest
        {
            Purpose = invalidPurpose,
            ContentType = "image/jpeg",
            SizeBytes = 1024
        };

        var act = () => _service.GetUploadUrlAsync(request, "user-123");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Mục đích upload*không hợp lệ*");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/heic")]
    public async Task GetUploadUrl_WithAllowedContentType_ShouldSucceed(string contentType)
    {
        _storageMock
            .Setup(s => s.GetPresignedPutUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), default))
            .ReturnsAsync("https://s3.mock/put");

        var request = new UploadUrlRequest
        {
            Purpose = MediaPurpose.Avatar,
            ContentType = contentType,
            SizeBytes = 1024 * 100
        };

        var response = await _service.GetUploadUrlAsync(request, "user-123");

        response.Should().NotBeNull();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/html")]
    [InlineData("application/x-msdownload")]
    [InlineData("image/svg+xml")]
    public async Task GetUploadUrl_WithDisallowedContentType_ShouldThrowArgumentException(string invalidContentType)
    {
        var request = new UploadUrlRequest
        {
            Purpose = MediaPurpose.Avatar,
            ContentType = invalidContentType,
            SizeBytes = 1024
        };

        var act = () => _service.GetUploadUrlAsync(request, "user-123");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Content-Type*không được hỗ trợ*");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(6 * 1024 * 1024)] // Avatar giới hạn 5MB
    public async Task GetUploadUrl_WithInvalidSize_ForAvatar_ShouldThrowArgumentException(long invalidSize)
    {
        var request = new UploadUrlRequest
        {
            Purpose = MediaPurpose.Avatar,
            ContentType = "image/jpeg",
            SizeBytes = invalidSize
        };

        var act = () => _service.GetUploadUrlAsync(request, "user-123");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Dung lượng ảnh*không hợp lệ*");
    }
}
