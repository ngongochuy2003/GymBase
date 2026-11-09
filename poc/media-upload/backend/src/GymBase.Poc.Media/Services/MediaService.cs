using System.Collections.Concurrent;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Processing;
using GymBase.Poc.Media.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Services;

public interface IMediaService
{
    Task<UploadUrlResponse> GetUploadUrlAsync(
        UploadUrlRequest request,
        string? ownerId,
        CancellationToken ct = default);

    Task<ConfirmUploadResponse> ConfirmUploadAsync(
        ConfirmUploadRequest request,
        string? ownerId,
        CancellationToken ct = default);

    Task<MediaAsset?> GetAssetByIdAsync(string mediaId, CancellationToken ct = default);

    Task<MediaAsset?> GetAssetByHashAsync(string purpose, string contentHash, CancellationToken ct = default);
}

public sealed class MediaService : IMediaService
{
    private readonly IMediaStorage _storage;
    private readonly IImageProcessor _imageProcessor;
    private readonly MediaOptions _options;
    private readonly ILogger<MediaService> _logger;

    // In-memory metadata store cho POC (chuẩn bị sẵn để map sang EF Core DbContext khi ghép vào API-01)
    private static readonly ConcurrentDictionary<string, MediaAsset> AssetsById = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, MediaAsset> AssetsByPurposeAndHash = new(StringComparer.Ordinal);

    public MediaService(
        IMediaStorage storage,
        IImageProcessor imageProcessor,
        IOptions<MediaOptions> options,
        ILogger<MediaService> logger)
    {
        _storage = storage;
        _imageProcessor = imageProcessor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadUrlResponse> GetUploadUrlAsync(
        UploadUrlRequest request,
        string? ownerId,
        CancellationToken ct = default)
    {
        // 1. Kiểm tra tính hợp lệ của mục đích upload
        if (!MediaPurpose.IsValid(request.Purpose))
        {
            throw new ArgumentException(
                $"Mục đích upload '{request.Purpose}' không hợp lệ. Cho phép: {string.Join(", ", MediaPurpose.All)}");
        }

        // 2. Whitelist Content-Type
        if (!_options.AllowedContentTypes.Contains(request.ContentType))
        {
            throw new ArgumentException(
                $"Content-Type '{request.ContentType}' không được hỗ trợ. Chỉ chấp nhận định dạng ảnh hợp lệ.");
        }

        // 3. Giới hạn dung lượng tối đa
        var maxAllowedBytes = MediaPurpose.GetMaxSizeBytes(request.Purpose);
        if (request.SizeBytes <= 0 || request.SizeBytes > maxAllowedBytes)
        {
            throw new ArgumentException(
                $"Dung lượng ảnh ({request.SizeBytes} bytes) không hợp lệ. Giới hạn cho {request.Purpose} là {maxAllowedBytes} bytes.");
        }

        // 4. Tạo staging key ngẫu nhiên và an toàn (không dùng tên file từ client để tránh path traversal / injection)
        var uploadId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var stagingKey = $"staging/{request.Purpose.ToLowerInvariant()}/{now:yyyy}/{now:MM}/{uploadId}.tmp";

        // 5. Sinh Presigned PUT URL có thời hạn ngắn (mặc định 15 phút) và ràng buộc Content-Type
        var expiry = TimeSpan.FromMinutes(_options.PresignExpiryMinutes);
        var presignedUrl = await _storage.GetPresignedPutUrlAsync(stagingKey, request.ContentType, expiry, ct);

        _logger.LogInformation(
            "Tạo Presigned PUT URL thành công: UploadId={UploadId}, Purpose={Purpose}, StagingKey={StagingKey}",
            uploadId, request.Purpose, stagingKey);

        return new UploadUrlResponse
        {
            UploadId = uploadId,
            PresignedPutUrl = presignedUrl,
            StagingKey = stagingKey,
            ExpiresInSeconds = (int)expiry.TotalSeconds
        };
    }

    public async Task<ConfirmUploadResponse> ConfirmUploadAsync(
        ConfirmUploadRequest request,
        string? ownerId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.StagingKey))
        {
            throw new ArgumentException("StagingKey không được để trống.");
        }

        if (!MediaPurpose.IsValid(request.Purpose))
        {
            throw new ArgumentException($"Mục đích upload '{request.Purpose}' không hợp lệ.");
        }

        // Bảo mật: Staging key phải bắt đầu đúng tiền tố staging của purpose tương ứng và không chứa path traversal
        var expectedPrefix = $"staging/{request.Purpose.ToLowerInvariant()}/";
        if (!request.StagingKey.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            || request.StagingKey.Contains("..")
            || request.StagingKey.Contains('\\'))
        {
            throw new UnauthorizedAccessException("StagingKey không hợp lệ hoặc chứa ký tự path traversal.");
        }

        // 1. Kiểm tra object trên S3 staging qua HeadObject (xác minh tồn tại và size thực tế)
        var stagingMeta = await _storage.HeadObjectAsync(request.StagingKey, ct);
        if (stagingMeta == null)
        {
            throw new FileNotFoundException(
                $"Object tạm không tồn tại trên storage ({request.StagingKey}). Mobile cần PUT dữ liệu trước khi gọi confirm.");
        }

        if (stagingMeta.SizeBytes == 0)
        {
            throw new InvalidDataException("Object được upload lên staging có kích thước 0 bytes.");
        }

        var maxAllowedBytes = MediaPurpose.GetMaxSizeBytes(request.Purpose);
        if (stagingMeta.SizeBytes > maxAllowedBytes)
        {
            // Xóa file rác vượt dung lượng trên staging ngay lập tức
            await _storage.DeleteObjectAsync(request.StagingKey, ct);
            throw new InvalidOperationException($"File vượt quá dung lượng tối đa cho phép ({maxAllowedBytes} bytes).");
        }

        // 2. Tải stream từ staging và kiểm tra magic bytes (không tin tưởng hoàn toàn header Content-Type)
        using var stream = await _storage.GetObjectStreamAsync(request.StagingKey, ct);
        _imageProcessor.ValidateMagicBytes(stream, stagingMeta.ContentType);

        // 3. Giải mã, kiểm tra chống decompression bomb, resize các biến thể (thumb, medium, large), encode WebP
        var processed = await _imageProcessor.ProcessVariantsAsync(stream, request.Purpose, ct);

        // 4. Cơ chế Deduplication theo Content Hash (SHA-256)
        var dedupeKey = $"{request.Purpose.ToLowerInvariant()}:{processed.ContentHash}";
        if (AssetsByPurposeAndHash.TryGetValue(dedupeKey, out var existingAsset))
        {
            _logger.LogInformation(
                "Phát hiện ảnh trùng lặp (SHA-256: {Hash}). Tái sử dụng Asset {AssetId}, không lưu thêm object mới.",
                processed.ContentHash, existingAsset.Id);

            // Dọn dẹp file staging mồ côi
            await _storage.DeleteObjectAsync(request.StagingKey, ct);

            var existingUrls = await BuildVariantUrlsAsync(existingAsset, ct);
            return new ConfirmUploadResponse
            {
                MediaId = existingAsset.Id,
                ContentHash = existingAsset.ContentHash,
                Urls = existingUrls,
                Width = existingAsset.Width,
                Height = existingAsset.Height,
                Blurhash = existingAsset.Blurhash,
                TotalSizeBytes = existingAsset.TotalSizeBytes,
                IsDeduplicated = true
            };
        }

        // 5. Lưu 3 biến thể ảnh vào key chính thức với Cache-Control immutable 1 năm
        var purposeLower = request.Purpose.ToLowerInvariant();
        var baseKey = $"media/{purposeLower}/{processed.ContentHash}";
        var thumbKey = $"{baseKey}/thumb.webp";
        var mediumKey = $"{baseKey}/medium.webp";
        var largeKey = $"{baseKey}/large.webp";

        const string cacheControl = "public, max-age=31536000, immutable";

        foreach (var variant in processed.Variants)
        {
            var variantKey = $"{baseKey}/{variant.VariantName}.webp";
            using var variantStream = new MemoryStream(variant.WebpBytes, writable: false);
            await _storage.PutObjectAsync(variantKey, variantStream, "image/webp", cacheControl, ct);
        }

        // 6. Xóa file gốc trong staging sau khi đã sinh biến thể thành công (tiết kiệm dung lượng S3 tối đa)
        await _storage.DeleteObjectAsync(request.StagingKey, ct);

        // 7. Tạo và lưu thực thể metadata
        var asset = new MediaAsset
        {
            Purpose = request.Purpose,
            ContentHash = processed.ContentHash,
            BaseKey = baseKey,
            ThumbKey = thumbKey,
            MediumKey = mediumKey,
            LargeKey = largeKey,
            Width = processed.OriginalWidth,
            Height = processed.OriginalHeight,
            TotalSizeBytes = processed.TotalVariantsSizeBytes,
            Blurhash = processed.Blurhash,
            OwnerId = ownerId,
            StorageClass = MediaPurpose.GetRecommendedStorageClass(request.Purpose),
            CreatedAt = DateTime.UtcNow
        };

        AssetsById[asset.Id] = asset;
        AssetsByPurposeAndHash[dedupeKey] = asset;

        var urls = await BuildVariantUrlsAsync(asset, ct);

        _logger.LogInformation(
            "Xác nhận upload thành công: MediaId={MediaId}, Hash={Hash}, TotalVariantsSize={Size} bytes",
            asset.Id, asset.ContentHash, asset.TotalSizeBytes);

        return new ConfirmUploadResponse
        {
            MediaId = asset.Id,
            ContentHash = asset.ContentHash,
            Urls = urls,
            Width = asset.Width,
            Height = asset.Height,
            Blurhash = asset.Blurhash,
            TotalSizeBytes = asset.TotalSizeBytes,
            IsDeduplicated = false
        };
    }

    public Task<MediaAsset?> GetAssetByIdAsync(string mediaId, CancellationToken ct = default)
    {
        AssetsById.TryGetValue(mediaId, out var asset);
        return Task.FromResult(asset);
    }

    public Task<MediaAsset?> GetAssetByHashAsync(string purpose, string contentHash, CancellationToken ct = default)
    {
        var dedupeKey = $"{purpose.ToLowerInvariant()}:{contentHash}";
        AssetsByPurposeAndHash.TryGetValue(dedupeKey, out var asset);
        return Task.FromResult(asset);
    }

    private async Task<ImageVariantUrls> BuildVariantUrlsAsync(MediaAsset asset, CancellationToken ct)
    {
        var thumbUrl = await ResolveUrlAsync(asset.ThumbKey, ct);
        var mediumUrl = await ResolveUrlAsync(asset.MediumKey, ct);
        var largeUrl = await ResolveUrlAsync(asset.LargeKey, ct);

        return new ImageVariantUrls
        {
            Thumb = thumbUrl,
            Medium = mediumUrl,
            Large = largeUrl
        };
    }

    private async Task<string> ResolveUrlAsync(string key, CancellationToken ct)
    {
        // Ưu tiên 1: CloudFront CDN Base URL (nếu có cấu hình)
        if (!string.IsNullOrWhiteSpace(_options.CdnBaseUrl))
        {
            return $"{_options.CdnBaseUrl.TrimEnd('/')}/{key}";
        }

        // Ưu tiên 2: Presigned GET URL (hoặc trực tiếp từ S3/MinIO)
        return await _storage.GetPresignedGetUrlAsync(key, TimeSpan.FromDays(7), ct);
    }
}
