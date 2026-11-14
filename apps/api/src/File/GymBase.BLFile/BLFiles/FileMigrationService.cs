using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using Microsoft.Extensions.Logging;

namespace GymBase.BLFile.BLFiles;

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

    public async Task<MigrationResultDto> BLMigrateLocalToS3Async(int batchSize = 50, CancellationToken ct = default)
    {
        var localProvider = _storageResolver.Resolve("LOCAL");
        var s3Provider = _storageResolver.Resolve("S3");

        var pendingFiles = await _tepTinRepository.DLGetPendingMigrationAsync("LOCAL", batchSize, ct);
        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        foreach (var tepTin in pendingFiles)
        {
            try
            {
                _logger.LogInformation("Đang migrate file {FileId} (Key: {Key}) sang S3...", tepTin.Id, tepTin.s3_key);

                await using var localStream = await localProvider.OpenReadAsync(tepTin.s3_key, ct);
                await s3Provider.SaveAsync(tepTin.s3_key, localStream, tepTin.loai_mime, ct);

                var s3Info = await s3Provider.GetInfoAsync(tepTin.s3_key, ct);
                if (s3Info == null || s3Info.Length != tepTin.kich_thuoc_byte)
                {
                    throw new InvalidOperationException($"Kích thước file trên S3 không khớp ({s3Info?.Length} != {tepTin.kich_thuoc_byte}).");
                }

                tepTin.s3_bucket = "S3";
                tepTin.ngay_xac_nhan = DateTime.UtcNow;
                await _tepTinRepository.DLUpdateAsync(tepTin, ct);

                successCount++;
                _logger.LogInformation("Migrate file {FileId} sang S3 thành công.", tepTin.Id);
            }
            catch (Exception ex)
            {
                failureCount++;
                var msg = $"Lỗi khi migrate file {tepTin.Id}: {ex.Message}";
                errors.Add(msg);
                _logger.LogError(ex, "Lỗi migrate file {FileId}", tepTin.Id);
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
