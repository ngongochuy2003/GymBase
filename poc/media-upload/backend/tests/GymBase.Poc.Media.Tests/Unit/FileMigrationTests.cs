using FluentAssertions;
using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.BL;
using GymBase.Poc.Media.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class FileMigrationTests
{
    private readonly ITepTinRepository _tepTinRepository;
    private readonly Mock<IStorageResolver> _storageResolverMock;
    private readonly Mock<IFileStorage> _localProviderMock;
    private readonly Mock<IFileStorage> _s3ProviderMock;
    private readonly FileMigrationService _migrationService;

    public FileMigrationTests()
    {
        _tepTinRepository = new InMemoryTepTinRepository();
        _storageResolverMock = new Mock<IStorageResolver>();
        _localProviderMock = new Mock<IFileStorage>();
        _s3ProviderMock = new Mock<IFileStorage>();

        _localProviderMock.SetupGet(p => p.ProviderName).Returns("LOCAL");
        _s3ProviderMock.SetupGet(p => p.ProviderName).Returns("S3");

        _storageResolverMock.Setup(r => r.Resolve("LOCAL")).Returns(_localProviderMock.Object);
        _storageResolverMock.Setup(r => r.Resolve("S3")).Returns(_s3ProviderMock.Object);

        _migrationService = new FileMigrationService(
            _tepTinRepository,
            _storageResolverMock.Object,
            NullLogger<FileMigrationService>.Instance);
    }

    [Fact]
    public async Task MigrateLocalToS3Async_WhenFilesExist_ShouldCopyAndSetProviderToS3()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var bytes = new byte[] { 10, 20, 30, 40 };
        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = Guid.NewGuid(),
            TenGoc = "photo.webp",
            KhoaLuuTru = "avatar/u1/photo.webp",
            NhaCungCap = "LOCAL",
            LoaiMime = "image/webp",
            DungLuong = bytes.Length,
            MucDich = "avatar",
            TrangThai = "READY",
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        await _tepTinRepository.CreateAsync(tepTin);

        _localProviderMock.Setup(p => p.OpenReadAsync("avatar/u1/photo.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(bytes));

        _s3ProviderMock.Setup(p => p.GetInfoAsync("avatar/u1/photo.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredObjectInfo(bytes.Length, "image/webp"));

        // Act
        var result = await _migrationService.MigrateLocalToS3Async(batchSize: 10);

        // Assert
        result.TotalScanned.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);

        _s3ProviderMock.Verify(s => s.SaveAsync(
            "avatar/u1/photo.webp",
            It.IsAny<Stream>(),
            "image/webp",
            It.IsAny<CancellationToken>()), Times.Once);

        var updated = await _tepTinRepository.GetByIdAsync(fileId);
        updated!.NhaCungCap.Should().Be("S3");
    }

    [Fact]
    public async Task MigrateLocalToS3Async_WhenS3Fails_ShouldKeepProviderAsLocal_AndReportFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3 };
        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = Guid.NewGuid(),
            TenGoc = "bad.webp",
            KhoaLuuTru = "avatar/bad.webp",
            NhaCungCap = "LOCAL",
            LoaiMime = "image/webp",
            DungLuong = bytes.Length,
            MucDich = "avatar",
            TrangThai = "READY",
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        await _tepTinRepository.CreateAsync(tepTin);

        _localProviderMock.Setup(p => p.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(bytes));

        _s3ProviderMock.Setup(p => p.SaveAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AWS S3 Connection Timeout"));

        // Act
        var result = await _migrationService.MigrateLocalToS3Async(batchSize: 10);

        // Assert
        result.TotalScanned.Should().Be(1);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(1);
        result.Errors.Should().NotBeEmpty();

        var untouched = await _tepTinRepository.GetByIdAsync(fileId);
        untouched!.NhaCungCap.Should().Be("LOCAL", "Nếu migrate lỗi thì giữ nguyên LOCAL để người dùng vẫn đọc được ảnh");
    }
}
