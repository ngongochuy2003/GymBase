using System.Security.Claims;
using FluentAssertions;
using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.BL;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Model;
using GymBase.Poc.Media.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SkiaSharp;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class FileServiceTests : IDisposable
{
    private readonly string _testTempDir;
    private readonly ITepTinRepository _tepTinRepository;
    private readonly Mock<IStorageResolver> _storageResolverMock;
    private readonly Mock<IFileStorage> _localStorageMock;
    private readonly IImageProcessor _imageProcessor;
    private readonly StorageOptions _storageOptions;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "gymbase_fileservice_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);

        _tepTinRepository = new InMemoryTepTinRepository();
        _storageResolverMock = new Mock<IStorageResolver>();
        _localStorageMock = new Mock<IFileStorage>();
        _localStorageMock.SetupGet(x => x.ProviderName).Returns("LOCAL");

        _storageResolverMock.Setup(r => r.GetDefault()).Returns(_localStorageMock.Object);
        _storageResolverMock.Setup(r => r.Resolve("LOCAL")).Returns(_localStorageMock.Object);

        _imageProcessor = new SkiaImageProcessor();

        _storageOptions = new StorageOptions
        {
            DefaultProvider = "LOCAL",
            LocalRoot = _testTempDir,
            AllowAnonymousForPoc = true,
            MaxUploadBytes = 10 * 1024 * 1024
        };

        _fileService = new FileService(
            _storageResolverMock.Object,
            _tepTinRepository,
            _imageProcessor,
            Options.Create(_storageOptions),
            NullLogger<FileService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_WithValidImage_ShouldSaveAndReturnReadyFile()
    {
        // Arrange
        var jpegBytes = CreateSampleJpeg(200, 200);
        var formFile = CreateFormFile(jpegBytes, "avatar.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        // Act
        var result = await _fileService.UploadAsync(user, formFile, "avatar");

        // Assert
        result.Should().NotBeNull();
        result.OriginalName.Should().Be("avatar.jpg");
        result.ContentType.Should().Be("image/webp");
        result.Purpose.Should().Be("avatar");
        result.Provider.Should().Be("LOCAL");
        result.Blurhash.Should().NotBeNullOrWhiteSpace();

        var entity = await _tepTinRepository.GetByIdAsync(result.FileId);
        entity.Should().NotBeNull();
        entity!.TrangThai.Should().Be("READY");
        entity.NhaCungCap.Should().Be("LOCAL");

        _localStorageMock.Verify(s => s.SaveAsync(
            It.Is<string>(key => key.StartsWith("avatar/")),
            It.IsAny<Stream>(),
            "image/webp",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WithEmptyFile_ShouldThrowArgumentException()
    {
        var formFile = CreateFormFile(Array.Empty<byte>(), "empty.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        Func<Task> act = async () => await _fileService.UploadAsync(user, formFile, "avatar");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*rỗng*");
    }

    [Fact]
    public async Task UploadAsync_WithInvalidPurpose_ShouldThrowArgumentException()
    {
        var jpegBytes = CreateSampleJpeg(50, 50);
        var formFile = CreateFormFile(jpegBytes, "pic.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        Func<Task> act = async () => await _fileService.UploadAsync(user, formFile, "invalid_purpose");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*không hợp lệ*");
    }

    [Fact]
    public async Task UploadAsync_WithFakeImage_ShouldThrowInvalidOperationException()
    {
        var fakeBytes = "This is a fake text file disguised as jpg"u8.ToArray();
        var formFile = CreateFormFile(fakeBytes, "fake.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        Func<Task> act = async () => await _fileService.UploadAsync(user, formFile, "avatar");

        await act.Should().ThrowAsync<InvalidDataException>()
            .WithMessage("*không khớp với magic bytes*");
    }

    [Fact]
    public async Task OpenAuthorizedAsync_PublicPurpose_ShouldAllowAccessForAnyUser()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = ownerId,
            TenGoc = "avatar.jpg",
            KhoaLuuTru = "avatar/test.webp",
            NhaCungCap = "LOCAL",
            LoaiMime = "image/webp",
            DungLuong = 100,
            MucDich = "avatar",
            TrangThai = "READY",
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        await _tepTinRepository.CreateAsync(tepTin);

        var dummyStream = new MemoryStream(new byte[] { 1, 2, 3 });
        _localStorageMock.Setup(s => s.OpenReadAsync("avatar/test.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyStream);

        var otherUser = CreateUser(otherUserId);

        // Act
        var result = await _fileService.OpenAuthorizedAsync(otherUser, fileId);

        // Assert
        result.Should().NotBeNull();
        result.ContentType.Should().Be("image/webp");
        result.OriginalName.Should().Be("avatar.jpg");
    }

    [Fact]
    public async Task OpenAuthorizedAsync_PrivateProgressImage_OtherUserWithoutAnon_ShouldThrowUnauthorized()
    {
        _storageOptions.AllowAnonymousForPoc = false;
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = ownerId,
            TenGoc = "progress.jpg",
            KhoaLuuTru = "progress/test.webp",
            NhaCungCap = "LOCAL",
            LoaiMime = "image/webp",
            DungLuong = 100,
            MucDich = "progress",
            TrangThai = "READY",
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        await _tepTinRepository.CreateAsync(tepTin);

        var otherUser = CreateUser(otherUserId);

        Func<Task> act = async () => await _fileService.OpenAuthorizedAsync(otherUser, fileId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*không có quyền*");
    }

    [Fact]
    public async Task DeleteAsync_AsOwner_ShouldDeletePhysicalAndLogical()
    {
        var ownerId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = ownerId,
            TenGoc = "pic.jpg",
            KhoaLuuTru = "avatar/to_del.webp",
            NhaCungCap = "LOCAL",
            LoaiMime = "image/webp",
            DungLuong = 100,
            MucDich = "avatar",
            TrangThai = "READY",
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        await _tepTinRepository.CreateAsync(tepTin);

        var user = CreateUser(ownerId);

        // Act
        await _fileService.DeleteAsync(user, fileId);

        // Assert
        _localStorageMock.Verify(s => s.DeleteAsync("avatar/to_del.webp", It.IsAny<CancellationToken>()), Times.Once);
        var inDb = await _tepTinRepository.GetByIdAsync(fileId);
        inDb!.NgayXoa.Should().NotBeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempDir))
        {
            try { Directory.Delete(_testTempDir, true); } catch { }
        }
    }

    private static ClaimsPrincipal CreateUser(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString())
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static byte[] CreateSampleJpeg(int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = surface.Canvas;
        canvas.Clear(SKColors.Blue);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data.ToArray();
    }
}
