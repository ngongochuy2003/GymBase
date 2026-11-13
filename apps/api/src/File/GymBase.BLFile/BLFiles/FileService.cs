using System.Security.Claims;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.Entities.File;
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
    private readonly IImageProcessor _imageProcessor;
    private readonly StorageOptions _options;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IStorageResolver storageResolver,
        ITepTinRepository tepTinRepository,
        IImageProcessor imageProcessor,
        IOptions<StorageOptions> options,
        ILogger<FileService> logger)
    {
        _storageResolver = storageResolver;
        _tepTinRepository = tepTinRepository;
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

        var userId = GetUserId(user);

        await using var rawStream = file.OpenReadStream();
        _imageProcessor.ValidateMagicBytes(rawStream, file.ContentType);

        var processed = await _imageProcessor.ProcessVariantsAsync(rawStream, normalizedPurpose, ct);
        var primaryVariant = processed.Variants.FirstOrDefault(v => v.VariantName == "medium")
                             ?? processed.Variants.First();

        var fileId = Guid.NewGuid();
        var storageKey = $"{normalizedPurpose}/{userId:N}/{fileId:N}.webp";

        var provider = _storageResolver.GetDefault();

        var entity = new tep_tin
        {
            Id = fileId,
            nguoi_tai_len_id = userId,
            ten_tep_goc = Path.GetFileName(file.FileName),
            khoa_luu_tru = storageKey,
            nha_cung_cap = provider.ProviderName,
            loai_mime = "image/webp",
            dung_luong = primaryVariant.WebpBytes.Length,
            muc_dich = normalizedPurpose,
            trang_thai = "UPLOADING",
            blurhash = processed.Blurhash,
            che_do_truy_cap = normalizedPurpose is "avatar" or "post" ? "cong_khai" : "rieng_tu",
            ngay_tao = DateTime.UtcNow
        };

        await _tepTinRepository.DLInsertAsync(entity, ct);

        try
        {
            using var saveStream = new MemoryStream(primaryVariant.WebpBytes, writable: false);
            await provider.SaveAsync(storageKey, saveStream, "image/webp", ct);

            entity.trang_thai = "READY";
            entity.ngay_cap_nhat = DateTime.UtcNow;
            await _tepTinRepository.DLUpdateAsync(entity, ct);

            _logger.LogInformation("Upload tệp tin thành công: Id={FileId}, Provider={Provider}, Key={Key}",
                fileId, provider.ProviderName, storageKey);

            return new FileUploadResultDto(
                FileId: entity.Id,
                OriginalName: entity.ten_tep_goc,
                ContentType: entity.loai_mime,
                Size: entity.dung_luong,
                Purpose: entity.muc_dich,
                Provider: entity.nha_cung_cap,
                Blurhash: entity.blurhash,
                ContentUrl: $"/api/files/{entity.Id}/content"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu tệp tin {FileId} xuống storage {Provider}.", fileId, provider.ProviderName);

            entity.trang_thai = "FAILED";
            entity.ngay_cap_nhat = DateTime.UtcNow;
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
        if (entity == null || entity.trang_thai != "READY")
        {
            throw new FileNotFoundException($"Tệp tin {id} không tồn tại hoặc chưa sẵn sàng.");
        }

        var currentUserId = GetUserId(user);
        var isOwner = entity.nguoi_tai_len_id == currentUserId;
        var isPublic = entity.che_do_truy_cap == "cong_khai" || entity.muc_dich is "avatar" or "post";

        if (!isOwner && !isPublic && !_options.AllowAnonymousForDev)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập tệp tin riêng tư này.");
        }

        var provider = _storageResolver.Resolve(entity.nha_cung_cap);
        var stream = await provider.OpenReadAsync(entity.khoa_luu_tru, ct);

        return new FileContentStreamDto(stream, entity.loai_mime, entity.ten_tep_goc);
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
            OriginalName: entity.ten_tep_goc,
            ContentType: entity.loai_mime,
            Size: entity.dung_luong,
            Purpose: entity.muc_dich,
            Provider: entity.nha_cung_cap,
            Status: entity.trang_thai,
            Blurhash: entity.blurhash,
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

        var currentUserId = GetUserId(user);
        if (entity.nguoi_tai_len_id != currentUserId && !_options.AllowAnonymousForDev)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa tệp tin này.");
        }

        var provider = _storageResolver.Resolve(entity.nha_cung_cap);
        await provider.DeleteAsync(entity.khoa_luu_tru, ct);

        await _tepTinRepository.DLDeleteAsync(id, ct);
        _logger.LogInformation("Đã xóa tệp tin {FileId} thành công.", id);
    }

    private Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(claim, out var parsed))
        {
            return parsed;
        }

        return _options.AllowAnonymousForDev
            ? Guid.Parse("11111111-1111-1111-1111-111111111111")
            : throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");
    }
}

