namespace GymBase.BLFile.BLFiles;

/// <summary>
/// Upload file theo hai bước và cấp URL đọc. App xin URL (<see cref="BLXinUploadAsync"/>), POST form thẳng lên MinIO/S3, rồi báo xong
/// (<see cref="BLXacNhanUploadAsync"/>). Thiết kế: docs/architecture/upload-file.md, quyết định: docs/adr/0003.
/// </summary>
public interface ITepTinService
{
    /// <summary>Tạo <c>tep_tin</c> ở <c>cho_tai_len</c> và ký form upload (presigned POST) vào key tạm.</summary>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">Mục đích, loại file hoặc dung lượng không hợp lệ.</exception>
    /// <exception cref="GymBase.Base.Exceptions.BusinessRuleException"><see cref="FileErrorCodes.QuotaExceeded"/>: hết hạn mức.</exception>
    Task<XinUploadResultDto> BLXinUploadAsync(Guid nguoiDungId, XinUploadRequestDto request, CancellationToken ct = default);

    /// <summary>Kiểm object đã lên storage đúng khai báo rồi chuyển sang <c>da_tai_len</c>. Gọi lại với file đã xác nhận thì trả kết quả hiện tại.</summary>
    /// <exception cref="GymBase.Base.Exceptions.NotFoundException">Không có file, hoặc file của người khác.</exception>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">Chưa upload, hoặc nội dung không khớp khai báo.</exception>
    Task<XacNhanUploadResultDto> BLXacNhanUploadAsync(Guid nguoiDungId, Guid fileId, CancellationToken ct = default);

    /// <summary>URL đọc một file. <exception cref="GymBase.Base.Exceptions.NotFoundException">Không có hoặc không được xem (không phân biệt, để khỏi lộ sự tồn tại).</exception></summary>
    Task<FileUrlDto> BLLayUrlAsync(Guid nguoiXemId, Guid fileId, CancellationToken ct = default);

    /// <summary>URL đọc nhiều file; file không có hoặc không được xem thì vắng mặt trong kết quả.</summary>
    Task<IReadOnlyDictionary<Guid, FileUrlDto>> BLLayUrlsAsync(
        Guid nguoiXemId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default);
}
