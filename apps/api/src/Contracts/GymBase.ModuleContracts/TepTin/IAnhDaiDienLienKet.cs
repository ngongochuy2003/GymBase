namespace GymBase.ModuleContracts.TepTin;

/// <summary>
/// Gắn ảnh đại diện vào hồ sơ cho nhóm Biz mà không phải tham chiếu BLFile. Ảnh được lưu trong <c>tep_tin</c> với
/// <c>loai_doi_tuong = ho_so_nguoi_dung</c>, <c>doi_tuong_id = tai_khoan_id</c>, <c>muc_dich = avatar</c>, chế độ công khai.
/// Gắn và gỡ tách làm hai bước để ảnh cũ chỉ bị xóa sau khi hồ sơ đã trỏ sang ảnh mới.
/// </summary>
public interface IAnhDaiDienLienKet
{
    /// <summary>Mục đích upload và giá trị <c>tep_tin.muc_dich</c> của ảnh đại diện.</summary>
    const string MucDich = "avatar";

    /// <summary>Gắn ảnh <paramref name="tepTinId"/> (chính chủ đã tải lên) vào hồ sơ; ảnh đang gắn rồi thì giữ nguyên.</summary>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">
    /// Tệp không tồn tại, không phải của chính chủ, không phải ảnh đại diện, hoặc đã bị xóa.
    /// </exception>
    Task BLGanAnhDaiDienAsync(Guid taiKhoanId, Guid tepTinId, CancellationToken cancellationToken);

    /// <summary>Xóa các ảnh đại diện khác <paramref name="giuLaiId"/> của tài khoản.</summary>
    Task BLGoAnhCuAsync(Guid taiKhoanId, Guid giuLaiId, CancellationToken cancellationToken);
}
