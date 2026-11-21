using System.Text.Json.Serialization;

namespace GymBase.ModuleContracts.TepTin;

/// <summary>Tệp chứng chỉ của PT đang gắn vào hồ sơ. Không có đường dẫn: tệp riêng tư, chỉ chủ hồ sơ đọc được qua API file.</summary>
public sealed record ChungChiPtDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("ten_tep")] string TenTep,
    [property: JsonPropertyName("kich_thuoc_byte")] long KichThuocByte);

/// <summary>
/// Gắn tệp chứng chỉ PT vào hồ sơ cho nhóm Biz (BLBiz lưu hồ sơ) mà không phải tham chiếu BLFile. Hiện thực nằm ở BLFile và
/// được host đăng ký. Tệp được lưu trong <c>tep_tin</c> với <c>loai_doi_tuong = ho_so_nguoi_dung</c>,
/// <c>doi_tuong_id = tai_khoan_id</c>, <c>muc_dich = chung_chi_pt</c>, chế độ riêng tư.
/// </summary>
public interface IChungChiPtLienKet
{
    /// <summary>Mục đích upload và giá trị <c>tep_tin.muc_dich</c> của chứng chỉ PT.</summary>
    const string MucDich = "chung_chi_pt";

    /// <summary>
    /// Đặt chứng chỉ của tài khoản: gắn <paramref name="tepTinId"/> (tệp chính chủ vừa tải lên) và gỡ chứng chỉ cũ;
    /// <paramref name="tepTinId"/> null thì chỉ gỡ chứng chỉ hiện có.
    /// </summary>
    /// <returns>Chứng chỉ sau khi đặt, hoặc null nếu không còn chứng chỉ.</returns>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">
    /// Tệp không tồn tại, không phải của chính chủ, không phải PDF chứng chỉ, hoặc đã bị xóa.
    /// </exception>
    Task<ChungChiPtDto?> BLDatChungChiAsync(Guid taiKhoanId, Guid? tepTinId, CancellationToken cancellationToken);

    /// <summary>Chứng chỉ đang gắn vào hồ sơ, null nếu chưa có.</summary>
    Task<ChungChiPtDto?> BLLayChungChiAsync(Guid taiKhoanId, CancellationToken cancellationToken);
}
