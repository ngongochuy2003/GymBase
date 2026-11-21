namespace GymBase.Base.Auth;

/// <summary>
/// Cho biết phiên đăng nhập của một access token còn hiệu lực không. Nằm ở Base để bước kiểm tra JWT (Base.Hosting) gọi
/// được mà không phụ thuộc nhóm Auth; hiện thực nằm ở BLAuth. Nhờ vậy đăng xuất, đổi mật khẩu, khóa tài khoản có hiệu
/// lực với access token ngay, không phải đợi token hết hạn.
/// </summary>
public interface IPhienHieuLucProvider
{
    /// <summary>true nếu phiên thuộc đúng tài khoản, chưa bị thu hồi và tài khoản còn hoạt động.</summary>
    /// <param name="sessionId">Claim <c>sid</c> của token.</param>
    /// <param name="taiKhoanId">Claim <c>sub</c> của token.</param>
    Task<bool> BLKiemTraConHieuLucAsync(Guid sessionId, Guid taiKhoanId, CancellationToken cancellationToken);
}
