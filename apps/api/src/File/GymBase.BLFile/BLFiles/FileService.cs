using System.Security.Claims;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

public sealed class FileService : IFileService
{
    private static readonly HashSet<string> AllowedPurposes = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatar", "gym_photo", "progress", "post"
    };

    private readonly IStorageResolver _storageResolver;
    private readonly ITepTinRepository _tepTinRepository;
    private readonly ITaiKhoanRepository _taiKhoanRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly StorageOptions _options;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IStorageResolver storageResolver,
        ITepTinRepository tepTinRepository,
        ITaiKhoanRepository taiKhoanRepository,
        IImageProcessor imageProcessor,
        IOptions<StorageOptions> options,
        ILogger<FileService> logger)
    {
        _storageResolver = storageResolver;
        _tepTinRepository = tepTinRepository;
        _taiKhoanRepository = taiKhoanRepository;
        _imageProcessor = imageProcessor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileUploadResultDto> BLUploadAsync(
        ClaimsPrincipal user,
        IFormFile file,
        string purpose,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Tệp tin tải lên rỗng.");
        }

        if (file.Length > _options.MaxUploadBytes)
        {
            throw new ArgumentException($"Dung lượng tệp tin ({file.Length} bytes) vượt quá giới hạn ({_options.MaxUploadBytes} bytes).");
        }

        var normalizedPurpose = purpose?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedPurposes.Contains(normalizedPurpose))
        {
            throw new ArgumentException($"Mục đích upload '{purpose}' không hợp lệ. Cho phép: {string.Join(", ", AllowedPurposes)}");
        }

        var userId = await GetUserIdAsync(user, ct);

        await using var rawStream = file.OpenReadStream();
        _imageProcessor.ValidateMagicBytes(rawStream, file.ContentType);

        var processed = await _imageProcessor.ProcessVariantsAsync(rawStream, normalizedPurpose, ct);
        var primaryVariant = processed.Variants.FirstOrDefault(v => v.VariantName == "medium")
                             ?? processed.Variants.First();

        var fileId = Guid.NewGuid();
        // Đổi tên ảnh thành mã ngẫu nhiên theo mục đích (avatar_20261114_..., post_20261114_...)
        // và phân loại theo từng folder riêng trên MinIO / S3 (avatar/, post/, ...)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomCode = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{normalizedPurpose}_{timestamp}_{randomCode}.webp";
        var storageKey = $"{normalizedPurpose}/{fileName}";

        var provider = _storageResolver.GetDefault();
        var bucketName = string.Equals(provider.ProviderName, "LOCAL", StringComparison.OrdinalIgnoreCase)
            ? "LOCAL"
            : (string.IsNullOrWhiteSpace(_options.BucketName) ? provider.ProviderName : _options.BucketName);
        var accessMode = normalizedPurpose is "avatar" or "post" ? CheDoTruyCap.cong_khai : CheDoTruyCap.rieng_tu;
        var targetType = normalizedPurpose switch
        {
            "avatar" => LoaiDoiTuongTepTin.ho_so_nguoi_dung,
            "post" => LoaiDoiTuongTepTin.bai_viet,
            _ => (LoaiDoiTuongTepTin?)null
        };

        var entity = new tep_tin
        {
            Id = fileId,
            nguoi_tai_len_id = userId,
            ten_tep_goc = Path.GetFileName(file.FileName),
            s3_key = storageKey,
            s3_bucket = bucketName,
            loai_mime = "image/webp",
            kich_thuoc_byte = primaryVariant.WebpBytes.Length,
            muc_dich = normalizedPurpose.Length > 32 ? normalizedPurpose[..32] : normalizedPurpose,
            loai_doi_tuong = targetType,
            trang_thai = TrangThaiTepTin.cho_tai_len,
            che_do_truy_cap = accessMode,
            ngay_tao = DateTime.UtcNow
        };

        await _tepTinRepository.DLInsertAsync(entity, ct);

        try
        {
            using var saveStream = new MemoryStream(primaryVariant.WebpBytes, writable: false);
            await provider.SaveAsync(storageKey, saveStream, "image/webp", ct);

            entity.trang_thai = TrangThaiTepTin.da_tai_len;
            entity.ngay_xac_nhan = DateTime.UtcNow;
            await _tepTinRepository.DLUpdateAsync(entity, ct);

            _logger.LogInformation("Upload tệp tin thành công: Id={FileId}, Bucket={Bucket}, Key={Key}",
                fileId, entity.s3_bucket, storageKey);

            return new FileUploadResultDto(
                FileId: entity.Id,
                OriginalName: entity.ten_tep_goc ?? file.FileName,
                ContentType: entity.loai_mime,
                Size: entity.kich_thuoc_byte,
                Purpose: entity.muc_dich ?? normalizedPurpose,
                Provider: provider.ProviderName,
                Blurhash: processed.Blurhash,
                ContentUrl: $"/api/v1/mob/files/{entity.Id}/content"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu tệp tin {FileId} xuống storage {Provider}.", fileId, provider.ProviderName);

            entity.trang_thai = TrangThaiTepTin.loi;
            entity.ngay_xac_nhan = DateTime.UtcNow;
            try { await _tepTinRepository.DLUpdateAsync(entity, ct); } catch { }

            try { await provider.DeleteAsync(storageKey, ct); } catch { }

            throw;
        }
    }

    public async Task<FileContentStreamDto> BLOpenAuthorizedAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _tepTinRepository.DLGetByIdAsync(id, ct);
        if (entity == null || entity.trang_thai is TrangThaiTepTin.loi or TrangThaiTepTin.da_xoa)
        {
            throw new FileNotFoundException($"Tệp tin {id} không tồn tại hoặc không khả dụng.");
        }

        var isPublic = entity.che_do_truy_cap == CheDoTruyCap.cong_khai || entity.muc_dich is "avatar" or "post";

        if (!isPublic)
        {
            var currentUserId = await GetUserIdAsync(user, ct);
            var isOwner = entity.nguoi_tai_len_id == currentUserId;
            if (!isOwner && !_options.AllowAnonymousForDev)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập tệp tin riêng tư này.");
            }
        }

        // Xử lý bản ghi external (liên kết tệp hoặc bài viết đồng bộ URL)
        var isExternal = string.Equals(entity.s3_bucket, "external", StringComparison.OrdinalIgnoreCase)
                         || entity.s3_key.StartsWith("external", StringComparison.OrdinalIgnoreCase)
                         || entity.s3_key.StartsWith("/api", StringComparison.OrdinalIgnoreCase);

        if (isExternal)
        {
            // 1. Trích xuất ID file gốc (vd: 626768cb-fe2b-4112-9c4d-13f83cc7713d từ chuỗi /api/v1/mob/files/{id}/content...)
            var match = System.Text.RegularExpressions.Regex.Match(
                entity.s3_key,
                @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})");

            if (match.Success && Guid.TryParse(match.Value, out var originFileId) && originFileId != entity.Id)
            {
                var originEntity = await _tepTinRepository.DLGetByIdAsync(originFileId, ct);
                if (originEntity != null && !string.Equals(originEntity.s3_bucket, "external", StringComparison.OrdinalIgnoreCase))
                {
                    var originProvider = _storageResolver.Resolve(originEntity.s3_bucket);
                    var originStream = await originProvider.OpenReadAsync(originEntity.s3_key, ct);
                    return new FileContentStreamDto(originStream, originEntity.loai_mime, originEntity.ten_tep_goc ?? $"{originEntity.Id}.webp");
                }
            }

            // 2. Nếu là URL tuyệt đối bên ngoài (http/https), tải và stream nội dung
            var cleanUrl = entity.s3_key;
            var hashIdx = cleanUrl.IndexOf('#');
            if (hashIdx >= 0) cleanUrl = cleanUrl[..hashIdx];
            if (cleanUrl.StartsWith("external/", StringComparison.OrdinalIgnoreCase))
            {
                cleanUrl = cleanUrl["external/".Length..];
            }

            if (cleanUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                cleanUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                var httpResponse = await httpClient.GetAsync(cleanUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var memStream = new MemoryStream();
                    await httpResponse.Content.CopyToAsync(memStream, ct);
                    memStream.Position = 0;
                    var mime = httpResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                    return new FileContentStreamDto(memStream, mime, entity.ten_tep_goc ?? "external.jpg");
                }
            }
        }

        var provider = _storageResolver.Resolve(entity.s3_bucket);
        var stream = await provider.OpenReadAsync(entity.s3_key, ct);

        return new FileContentStreamDto(stream, entity.loai_mime, entity.ten_tep_goc ?? $"{entity.Id}.webp");
    }

    public async Task<TepTinMetadataDto?> BLGetMetadataAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _tepTinRepository.DLGetByIdAsync(id, ct);
        if (entity == null)
        {
            return null;
        }

        return new TepTinMetadataDto(
            Id: entity.Id,
            OriginalName: entity.ten_tep_goc ?? $"{entity.Id}.webp",
            ContentType: entity.loai_mime,
            Size: entity.kich_thuoc_byte,
            Purpose: entity.muc_dich ?? string.Empty,
            Provider: entity.s3_bucket,
            Status: entity.trang_thai.ToString(),
            Blurhash: null,
            CreatedAt: entity.ngay_tao
        );
    }

    public async Task BLDeleteAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _tepTinRepository.DLGetByIdAsync(id, ct);
        if (entity == null)
        {
            return;
        }

        var currentUserId = await GetUserIdAsync(user, ct);
        if (entity.nguoi_tai_len_id != currentUserId && !_options.AllowAnonymousForDev)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa tệp tin này.");
        }

        var provider = _storageResolver.Resolve(entity.s3_bucket);
        await provider.DeleteAsync(entity.s3_key, ct);

        entity.trang_thai = TrangThaiTepTin.da_xoa;
        entity.ngay_xac_nhan = DateTime.UtcNow;
        await _tepTinRepository.DLUpdateAsync(entity, ct);
        _logger.LogInformation("Đã xóa tệp tin {FileId} thành công.", id);
    }

    private async Task<Guid> GetUserIdAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(claim, out var parsed))
        {
            return parsed;
        }

        if (_options.AllowAnonymousForDev)
        {
            var devAccount = await _taiKhoanRepository.DLFindByTenDangNhapAsync("admin", ct);
            if (devAccount != null)
            {
                return devAccount.Id;
            }
        }

        throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");
    }
}
