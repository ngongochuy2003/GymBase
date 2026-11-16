using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract.DLHoSoNguoiDung;

/// <summary>
/// Truy cập hồ sơ người dùng. Khóa chính của bảng là <c>tai_khoan_id</c> nên không dùng
/// <c>IBaseRepository</c> (cần <c>Id</c>).
/// </summary>
public interface IHoSoNguoiDungRepository
{
    /// <summary>Hồ sơ của tài khoản; null nếu tài khoản chưa chọn nhóm.</summary>
    Task<ho_so_nguoi_dung?> DLFindByTaiKhoanIdAsync(Guid taiKhoanId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi hồ sơ (thêm nếu chưa có, cập nhật nếu đã có) và chỉ số cơ thể mới trong **một transaction**.
    /// <paramref name="chiSoMoi"/> null khi số đo không đổi.
    /// </summary>
    Task DLUpsertAsync(ho_so_nguoi_dung hoSo, chi_so_co_the? chiSoMoi, CancellationToken cancellationToken);
}
