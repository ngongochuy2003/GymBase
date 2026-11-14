using System.Security.Claims;
using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;

namespace GymBase.UnitTests.File;

public class FileServiceTests : IDisposable
{
    private readonly string _testTempDir;
    private readonly InMemoryTepTinRepository _repository;
    private readonly FileService _service;

    public FileServiceTests()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "gymbase_fileservice_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);

        var options = new StorageOptions
        {
            DefaultProvider = "LOCAL",
            LocalRoot = _testTempDir,
            AllowAnonymousForDev = true,
            MaxUploadBytes = 10 * 1024 * 1024
        };

        var localStorage = new LocalFileStorage(Options.Create(options));
        var s3Storage = new S3FileStorage(new Amazon.S3.AmazonS3Client(new Amazon.Runtime.AnonymousAWSCredentials(), new Amazon.S3.AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.USEast1 }), Options.Create(options), NullLogger<S3FileStorage>.Instance);
        var resolver = new StorageResolver(localStorage, s3Storage, Options.Create(options));

        _repository = new InMemoryTepTinRepository(new InMemoryStore<tep_tin>());
        var processor = new SkiaImageProcessor();

        _service = new FileService(
            resolver,
            _repository,
            new InMemoryTaiKhoanRepository(new InMemoryStore<tai_khoan>()),
            processor,
            Options.Create(options),
            NullLogger<FileService>.Instance);
    }

    [Fact]
    public async Task BLUploadAsync_ValidImage_ShouldSaveAndReturnReadyMetadata()
    {
        var jpegBytes = CreateSampleJpeg(100, 100);
        var formFile = CreateFormFile(jpegBytes, "avatar.jpg", "image/jpeg");
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);

        var result = await _service.BLUploadAsync(user, formFile, "avatar");

        Assert.NotNull(result);
        Assert.Equal("avatar.jpg", result.OriginalName);
        Assert.Equal("image/webp", result.ContentType);
        Assert.Equal("avatar", result.Purpose);
        Assert.Equal("LOCAL", result.Provider);
        Assert.False(string.IsNullOrWhiteSpace(result.Blurhash));

        var dbEntity = await _repository.DLGetByIdAsync(result.FileId, CancellationToken.None);
        Assert.NotNull(dbEntity);
        Assert.Equal(TrangThaiTepTin.da_tai_len, dbEntity.trang_thai);
        // Kiểm tra đúng định dạng tên ngẫu nhiên theo mục đích và phân thư mục avatar/avatar_*.webp
        Assert.StartsWith("avatar/avatar_", dbEntity.s3_key);
        Assert.EndsWith(".webp", dbEntity.s3_key);
    }

    [Fact]
    public async Task BLUploadAsync_PostImage_ShouldPartitionIntoPostFolderWithRandomizedName()
    {
        var jpegBytes = CreateSampleJpeg(100, 100);
        var formFile = CreateFormFile(jpegBytes, "workout.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        var result = await _service.BLUploadAsync(user, formFile, "post");

        Assert.NotNull(result);
        Assert.Equal("post", result.Purpose);

        var dbEntity = await _repository.DLGetByIdAsync(result.FileId, CancellationToken.None);
        Assert.NotNull(dbEntity);
        Assert.StartsWith("post/post_", dbEntity.s3_key);
        Assert.EndsWith(".webp", dbEntity.s3_key);
    }

    [Fact]
    public async Task BLUploadAsync_FakeImage_ShouldThrowInvalidDataException()
    {
        var fakeBytes = "This is fake text file pretending to be jpg"u8.ToArray();
        var formFile = CreateFormFile(fakeBytes, "fake.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            _service.BLUploadAsync(user, formFile, "avatar"));
    }

    [Fact]
    public async Task BLUploadAsync_InvalidPurpose_ShouldThrowArgumentException()
    {
        var jpegBytes = CreateSampleJpeg(50, 50);
        var formFile = CreateFormFile(jpegBytes, "img.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.BLUploadAsync(user, formFile, "invalid_purpose"));
    }

    [Fact]
    public async Task BLOpenAuthorizedAsync_ExternalRecordReferencingOriginFile_ShouldResolveOriginFileStream()
    {
        var jpegBytes = CreateSampleJpeg(80, 80);
        var formFile = CreateFormFile(jpegBytes, "origin.jpg", "image/jpeg");
        var user = CreateUser(Guid.NewGuid());

        var originUpload = await _service.BLUploadAsync(user, formFile, "post");

        // Tạo bản ghi external mô phỏng việc DLSyncPostMedia đồng bộ URL
        var externalFileId = Guid.NewGuid();
        var externalEntity = new tep_tin
        {
            Id = externalFileId,
            nguoi_tai_len_id = Guid.NewGuid(),
            s3_bucket = "external",
            s3_key = $"/api/v1/mob/files/{originUpload.FileId}/content#tep={externalFileId:N}",
            ten_tep_goc = "post-media-1.jpg",
            loai_mime = "image/jpeg",
            che_do_truy_cap = CheDoTruyCap.cong_khai,
            trang_thai = TrangThaiTepTin.da_lien_ket,
            muc_dich = "post",
            ngay_tao = DateTime.UtcNow,
            ngay_xac_nhan = DateTime.UtcNow
        };
        await _repository.DLInsertAsync(externalEntity, CancellationToken.None);

        var streamResult = await _service.BLOpenAuthorizedAsync(user, externalFileId);

        Assert.NotNull(streamResult);
        Assert.NotNull(streamResult.Stream);
        Assert.Equal("image/webp", streamResult.ContentType);
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
        canvas.Clear(SKColors.CornflowerBlue);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return data.ToArray();
    }
}
