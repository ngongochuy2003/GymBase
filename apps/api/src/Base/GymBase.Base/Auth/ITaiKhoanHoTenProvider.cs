namespace GymBase.Base.Auth;

/// <summary>
/// Tên hiển thị của tài khoản cho nhóm khác (nhóm Biz không được đọc entity của nhóm Auth). Hiện thực nằm ở BLAuth.
/// </summary>
public interface ITaiKhoanHoTenProvider
{
    /// <summary><c>ho_ten</c> của tài khoản; null nếu không tìm thấy.</summary>
    Task<string?> BLDocHoTenAsync(Guid taiKhoanId, CancellationToken cancellationToken);
}
