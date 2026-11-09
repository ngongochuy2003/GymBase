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

public class MediaKeyBuilderTests
{
    private readonly MediaService _service;
    private readonly Mock<IMediaStorage> _storageMock;

    public MediaKeyBuilderTests()
    {
        _storageMock = new Mock<IMediaStorage>();
        _service = new MediaService(
            _storageMock.Object,
            Mock.Of<IImageProcessor>(),
            Options.Create(new MediaOptions()),
            NullLogger<MediaService>.Instance);
    }

    [Fact]
    public async Task StagingKey_ShouldFollowFormat_AndNotContainUserProvidedFilename()
    {
        _storageMock
            .Setup(s => s.GetPresignedPutUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), default))
            .ReturnsAsync("https://s3.mock/presigned-put");

        var request = new UploadUrlRequest
        {
            Purpose = MediaPurpose.GymPhoto,
            ContentType = "image/png",
            SizeBytes = 1024 * 1024
        };

        var response = await _service.GetUploadUrlAsync(request, "user-123");

        // Format: staging/gym_photo/{yyyy}/{MM}/{guid}.tmp
        response.StagingKey.Should().MatchRegex(@"^staging/gym_photo/\d{4}/\d{2}/[a-f0-9]{32}\.tmp$");
        response.StagingKey.Should().NotContain("user-123");
        response.StagingKey.Should().NotContain(" ");
        response.StagingKey.Should().NotContain("..");
    }

    [Fact]
    public void PersistentKeys_ShouldFollow_HashBasedStructure()
    {
        const string purpose = "progress_photo";
        const string hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        var baseKey = $"media/{purpose}/{hash}";
        var thumbKey = $"{baseKey}/thumb.webp";
        var mediumKey = $"{baseKey}/medium.webp";
        var largeKey = $"{baseKey}/large.webp";

        thumbKey.Should().Be("media/progress_photo/e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855/thumb.webp");
        mediumKey.Should().Be("media/progress_photo/e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855/medium.webp");
        largeKey.Should().Be("media/progress_photo/e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855/large.webp");
    }
}
