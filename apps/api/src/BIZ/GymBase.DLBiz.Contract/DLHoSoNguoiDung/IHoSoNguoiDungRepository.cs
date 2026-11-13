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
}
