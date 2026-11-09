using System.Security.Claims;
using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Model;
using GymBase.Poc.Media.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.BL;

/// <summary>
/// Nghiệp vụ quản lý file GymBase V1:
/// - Kiểm quyền và xác thực người dùng
/// - Kiểm tra Magic Bytes chống giả mạo
/// - Tối ưu hóa ảnh sang WebP và sinh Blurhash
/// - Quản lý trạng thái vòng đời trong bảng tep_tin
/// - Lưu trữ qua IStorageResolver (Local trước, S3 sau)
/// </summary>
public sealed class FileService : IFileService
{
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

    public async Task<FileUploadResultDto> UploadAsync(
        ClaimsPrincipal user,
        IFormFile file,
        string purpose,
        CancellationToken ct = default)
    {
        // 1. Kiểm tra file cơ bản
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Tệp tin tải lên rỗng.");
        }

        if (file.Length > _options.MaxUploadBytes)
        {
            throw new ArgumentException($"Dung lượng tệp tin ({file.Length} bytes) vượt quá giới hạn cho phép ({_options.MaxUploadBytes} bytes).");
        }

        // 2. Kiểm tra mục đích upload
        var normalizedPurpose = purpose?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!MediaPurpose.IsValid(normalizedPurpose))
        {
            throw new ArgumentException($"Mục đích upload '{purpose}' không hợp lệ. Cho phép: {string.Join(", ", MediaPurpose.All)}");
        }

        var maxPurposeSize = MediaPurpose.GetMaxSizeBytes(normalizedPurpose);
        if (file.Length > maxPurposeSize)
        {
            throw new ArgumentException($"Dung lượng tệp tin vượt quá giới hạn tối đa cho mục đích {normalizedPurpose} ({maxPurposeSize} bytes).");
        }

        // 3. Trích xuất User ID
        var userId = GetUserId(user);

        // 4. Đọc stream & kiểm tra magic bytes
        await using var rawStream = file.OpenReadStream();
        _imageProcessor.ValidateMagicBytes(rawStream, file.ContentType);

        // 5. Tối ưu hóa ảnh sang WebP (Medium max 1080px cho mục đích lưu trữ đa dụng) và sinh Blurhash
        var processed = await _imageProcessor.ProcessVariantsAsync(rawStream, normalizedPurpose, ct);
        var primaryVariant = processed.Variants.FirstOrDefault(v => v.VariantName == "medium")
                             ?? processed.Variants.First();

        var fileId = Guid.NewGuid();
        var storageKey = $"{normalizedPurpose}/{userId:N}/{fileId:N}.webp";

        // 6. Lấy Storage Provider mặc định (LOCAL ở giai đoạn V1)
        var provider = _storageResolver.GetDefault();

        var tepTin = new TepTin
        {
            Id = fileId,
            NguoiTaiLenId = userId,
            TenGoc = Path.GetFileName(file.FileName),
            KhoaLuuTru = storageKey,
            NhaCungCap = provider.ProviderName,
            LoaiMime = "image/webp",
            DungLuong = primaryVariant.WebpBytes.Length,
            MucDich = normalizedPurpose,
            TrangThai = "UPLOADING",
            Blurhash = processed.Blurhash,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        // Ghi metadata UPLOADING vào DB
        await _tepTinRepository.CreateAsync(tepTin, ct);

        try
        {
            // 7. Ghi nhị phân xuống Storage
            using var saveStream = new MemoryStream(primaryVariant.WebpBytes, writable: false);
            await provider.SaveAsync(storageKey, saveStream, "image/webp", ct);

            // 8. Đổi trạng thái READY
            tepTin.TrangThai = "READY";
            await _tepTinRepository.UpdateAsync(tepTin, ct);

            _logger.LogInformation("Upload tệp tin thành công: Id={FileId}, Provider={Provider}, Key={Key}",
                fileId, provider.ProviderName, storageKey);

            return new FileUploadResultDto(
                FileId: tepTin.Id,
                OriginalName: tepTin.TenGoc,
                ContentType: tepTin.LoaiMime,
                Size: tepTin.DungLuong,
                Purpose: tepTin.MucDich,
                Provider: tepTin.NhaCungCap,
                Blurhash: tepTin.Blurhash,
                ContentUrl: $"/api/files/{tepTin.Id}/content"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu tệp tin {FileId} xuống storage {Provider}. Đánh dấu FAILED.",
                fileId, provider.ProviderName);

            tepTin.TrangThai = "FAILED";
            await _tepTinRepository.UpdateAsync(tepTin, ct);

            // Xóa dọn file rác best-effort
            try
            {
                await provider.DeleteAsync(storageKey, ct);
            }
            catch
            {
                // Bỏ qua lỗi xóa rác
            }

            throw;
        }
    }

    public async Task<FileContentStreamDto> OpenAuthorizedAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var tepTin = await _tepTinRepository.GetByIdAsync(id, ct);
        if (tepTin == null || tepTin.TrangThai != "READY" || tepTin.NgayXoa != null)
        {
            throw new FileNotFoundException($"Tệp tin {id} không tồn tại hoặc chưa sẵn sàng.");
        }

        // Kiểm tra phân quyền truy cập
        var currentUserId = GetUserId(user);
        var isOwner = tepTin.NguoiTaiLenId == currentUserId;
        var isPublic = tepTin.MucDich is "avatar" or "post";

        if (!isOwner && !isPublic && !_options.AllowAnonymousForPoc)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập tệp tin riêng tư này.");
        }

        // Chọn provider phù hợp dựa trên nha_cung_cap của tệp tin này
        var provider = _storageResolver.Resolve(tepTin.NhaCungCap);
        var stream = await provider.OpenReadAsync(tepTin.KhoaLuuTru, ct);

        return new FileContentStreamDto(stream, tepTin.LoaiMime, tepTin.TenGoc);
    }

    public async Task<TepTinMetadataDto?> GetMetadataAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var tepTin = await _tepTinRepository.GetByIdAsync(id, ct);
        if (tepTin == null || tepTin.NgayXoa != null)
        {
            return null;
        }

        return new TepTinMetadataDto(
            Id: tepTin.Id,
            OriginalName: tepTin.TenGoc,
            ContentType: tepTin.LoaiMime,
            Size: tepTin.DungLuong,
            Purpose: tepTin.MucDich,
            Provider: tepTin.NhaCungCap,
            Status: tepTin.TrangThai,
            Blurhash: tepTin.Blurhash,
            CreatedAt: tepTin.NgayTao
        );
    }

    public async Task DeleteAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default)
    {
        var tepTin = await _tepTinRepository.GetByIdAsync(id, ct);
        if (tepTin == null || tepTin.NgayXoa != null)
        {
            return;
        }

        var currentUserId = GetUserId(user);
        if (tepTin.NguoiTaiLenId != currentUserId && !_options.AllowAnonymousForPoc)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa tệp tin này.");
        }

        tepTin.TrangThai = "DELETING";
        await _tepTinRepository.UpdateAsync(tepTin, ct);

        var provider = _storageResolver.Resolve(tepTin.NhaCungCap);
        await provider.DeleteAsync(tepTin.KhoaLuuTru, ct);

        await _tepTinRepository.DeleteAsync(id, ct);

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

        return _options.AllowAnonymousForPoc
            ? Guid.Parse("11111111-1111-1111-1111-111111111111")
            : throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");
    }
}
