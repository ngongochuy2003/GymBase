using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

/// <summary>
/// Xử lý nền sau khi tệp được xác nhận: với ảnh JPEG, PNG, WebP thì xoay đúng chiều, bỏ metadata (EXIF, GPS), ghi đè lại tệp và lưu blurhash.
/// Hàng đợi là cột <c>trang_thai_xu_ly</c> của <c>tep_tin</c>; worker nhận việc bằng cập nhật có điều kiện nên nhiều instance không xử lý trùng.
/// </summary>
public sealed class TepTinXuLyService(
    ITepTinRepository tepTins,
    IStorageResolver storageResolver,
    IImageProcessor imageProcessor,
    IOptions<StorageOptions> options,
    TimeProvider timeProvider,
    ILogger<TepTinXuLyService> logger)
{
    private readonly StorageOptions _options = options.Value;

    /// <summary>Xử lý một lượt các tệp đang chờ; trả số tệp đã nhận và xử lý (kể cả khi kết quả là lỗi).</summary>
    public async Task<int> BLXuLyTepChoXuLyAsync(CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cacTep = await tepTins.DLFindPendingProcessingAsync(
            now.AddMinutes(-_options.ProcessingReclaimMinutes), _options.ProcessingBatchSize, ct);

        var daXuLy = 0;
        foreach (var tep in cacTep)
        {
            if (!await tepTins.DLTryClaimProcessingAsync(tep, now, ct))
            {
                continue;
            }

            await XuLyMotTepAsync(tep, ct);
            daXuLy++;
        }

        return daXuLy;
    }

    private async Task XuLyMotTepAsync(tep_tin tep, CancellationToken ct)
    {
        // Đọc lại sau khi nhận việc: dòng có thể đã đổi (ví dụ bị dọn, hoặc người dùng xóa).
        var hienTai = await tepTins.DLGetByIdAsync(tep.Id, ct);
        if (hienTai == null)
        {
            return;
        }

        var storage = storageResolver.Resolve(hienTai.s3_bucket);
        try
        {
            await using var goc = await storage.OpenReadAsync(hienTai.s3_key, ct);
            using var boNho = new MemoryStream();
            await goc.CopyToAsync(boNho, ct);
            boNho.Position = 0;

            var anh = await imageProcessor.XuLyAsync(boNho, hienTai.loai_mime, ct);

            await using var moi = new MemoryStream(anh.Data);
            await storage.SaveAsync(hienTai.s3_key, moi, hienTai.loai_mime, ct);

            hienTai.kich_thuoc_byte = anh.Data.Length;
            hienTai.blurhash = anh.Blurhash;
            hienTai.trang_thai_xu_ly = TrangThaiXuLyTepTin.xong;
            await tepTins.DLUpdateAsync(hienTai, ct);
        }
        catch (Exception ex) when (ex is ImageTooLargeException or InvalidDataException)
        {
            // Ảnh quá lớn hoặc hỏng: từ chối hẳn, không để người khác xem. Job dọn file sẽ xóa dòng sau.
            logger.LogWarning(ex, "Từ chối ảnh {FileId}: {Message}", hienTai.Id, ex.Message);
            await storage.DeleteAsync(hienTai.s3_key, ct);
            hienTai.trang_thai = TrangThaiTepTin.loi;
            hienTai.trang_thai_xu_ly = TrangThaiXuLyTepTin.loi;
            await tepTins.DLUpdateAsync(hienTai, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Lỗi bất ngờ (kho, bộ nhớ): đánh dấu lỗi để khỏi lặp vô hạn; tệp vẫn xem được nhưng chưa bỏ EXIF.
            logger.LogError(ex, "Xử lý ảnh {FileId} lỗi.", hienTai.Id);
            hienTai.trang_thai_xu_ly = TrangThaiXuLyTepTin.loi;
            await tepTins.DLUpdateAsync(hienTai, ct);
        }
    }
}
