using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

/// <summary>
/// Dọn tệp mồ côi: tệp xin upload mà không bao giờ được gửi hoặc xác nhận (<c>cho_tai_len</c>, <c>loi</c>), và tệp đã upload mà không được gắn
/// vào đâu (<c>da_tai_len</c>). Xóa object (cả key tạm lẫn key chính) rồi xóa dòng. Chạy lại an toàn: object nào xóa lỗi thì giữ dòng để lượt sau
/// thử lại. Tệp đã gắn (<c>da_lien_ket</c>) không bao giờ bị dọn.
/// </summary>
public sealed class TepTinDonDepService(
    ITepTinRepository tepTins,
    IStorageResolver storageResolver,
    IOptions<StorageOptions> options,
    TimeProvider timeProvider,
    ILogger<TepTinDonDepService> logger)
{
    private readonly StorageOptions _options = options.Value;

    /// <summary>Dọn một lượt; trả số tệp đã xóa.</summary>
    public async Task<int> BLDonTepMoCoiAsync(CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var choTaiLenTruoc = now.AddMinutes(-_options.PendingMaxAgeMinutes);
        var daTaiLenTruoc = now.AddHours(-_options.UnlinkedMaxAgeHours);

        var cacTep = await tepTins.DLFindStaleAsync(choTaiLenTruoc, daTaiLenTruoc, _options.CleanupBatchSize, ct);
        var daXoa = 0;
        foreach (var tep in cacTep)
        {
            try
            {
                var storage = storageResolver.Resolve(tep.s3_bucket);
                await storage.DeleteAsync(FilePolicy.KeyTam(tep.Id), ct);
                await storage.DeleteAsync(tep.s3_key, ct);
                await tepTins.DLDeleteAsync(tep.Id, ct);
                daXoa++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Không dọn được tệp {FileId} ({TrangThai}); thử lại ở lượt sau.", tep.Id, tep.trang_thai);
            }
        }

        if (daXoa > 0)
        {
            logger.LogInformation("Đã dọn {Count} tệp mồ côi.", daXoa);
        }

        return daXoa;
    }
}
