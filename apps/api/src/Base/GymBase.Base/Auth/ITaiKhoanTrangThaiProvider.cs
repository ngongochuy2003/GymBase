namespace GymBase.Base.Auth;

/// <summary>
/// Trạng thái tài khoản mà nhóm khác cần biết (nhóm Biz không được đọc entity của nhóm Auth). Hiện thực nằm ở BLAuth.
/// </summary>
public interface ITaiKhoanTrangThaiProvider
{
    /// <summary>true nếu tài khoản tồn tại và chưa bị khóa (<c>dang_hoat_dong</c>).</summary>
    Task<bool> BLKiemTraHoatDongAsync(Guid taiKhoanId, CancellationToken cancellationToken);
}
