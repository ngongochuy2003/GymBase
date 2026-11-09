using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.Model;
using Microsoft.Extensions.Logging;

namespace GymBase.Poc.Media.BL;

public interface IFileMigrationService
{
    /// <summary>
    /// Di chuyển hàng loạt các file từ LOCAL sang S3 theo từng đợt (batch) an toàn, không downtime.
    /// </summary>
    Task<MigrationResultDto> MigrateLocalToS3Async(int batchSize = 50, CancellationToken ct = default);
}

public sealed class FileMigrationService : IFileMigrationService
{
    private readonly ITepTinRepository _tepTinRepository;
    private readonly IStorageResolver _storageResolver;
    private readonly ILogger<FileMigrationService> _logger;

    public FileMigrationService(
        ITepTinRepository tepTinRepository,
        IStorageResolver storageResolver,
        ILogger<FileMigrationService> logger)
    {
        _tepTinRepository = tepTinRepository;
        _storageResolver = storageResolver;
        _logger = logger;
    }

    public async Task<MigrationResultDto> MigrateLocalToS3Async(int batchSize = 50, CancellationToken ct = default)
    {
        var localProvider = _storageResolver.Resolve("LOCAL");
        var s3Provider = _storageResolver.Resolve("S3");

        var pendingFiles = await _tepTinRepository.GetPendingMigrationAsync("LOCAL", batchSize, ct);
        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        foreach (var tepTin in pendingFiles)
        {
            try
            {
                _logger.LogInformation("Đang migrate file {FileId} (Key: {Key}) sang S3...", tepTin.Id, tepTin.KhoaLuuTru);

                // 1. Mở stream đọc từ Local Storage
                await using var localStream = await localProvider.OpenReadAsync(tepTin.KhoaLuuTru, ct);

                // 2. Lưu sang S3
                await s3Provider.SaveAsync(tepTin.KhoaLuuTru, localStream, tepTin.LoaiMime, ct);

                // 3. Kiểm tra xác minh file trên S3
                var s3Info = await s3Provider.GetInfoAsync(tepTin.KhoaLuuTru, ct);
                if (s3Info == null || s3Info.Length != tepTin.DungLuong)
                {
                    throw new InvalidOperationException($"Kích thước file trên S3 không khớp ({s3Info?.Length} != {tepTin.DungLuong}).");
                }

                // 4. Cập nhật metadata nha_cung_cap = S3
                tepTin.NhaCungCap = "S3";
                await _tepTinRepository.UpdateAsync(tepTin, ct);

                successCount++;
                _logger.LogInformation("Migrate file {FileId} sang S3 thành công.", tepTin.Id);
            }
            catch (Exception ex)
            {
                failureCount++;
                var msg = $"Lỗi khi migrate file {tepTin.Id}: {ex.Message}";
                errors.Add(msg);
                _logger.LogError(ex, "Lỗi migrate file {FileId}", tepTin.Id);
                // Giữ nguyên nha_cung_cap = LOCAL để file vẫn đọc được bình thường
            }
        }

        return new MigrationResultDto(
            TotalScanned: pendingFiles.Count,
            SuccessCount: successCount,
            FailureCount: failureCount,
            Errors: errors
        );
    }
}
