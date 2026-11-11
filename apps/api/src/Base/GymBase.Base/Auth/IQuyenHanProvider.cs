namespace GymBase.Base.Auth;

/// <summary>
/// Cho biết tài khoản làm được gì ngay lúc này. Nằm ở Base để policy (Base.Hosting) gọi được mà không phụ thuộc nhóm Biz;
/// hiện thực nằm ở BLBiz. Đọc database mỗi lần gọi, vì quyền có thể đổi giữa hai request.
/// </summary>
public interface IQuyenHanProvider
{
    /// <param name="taiKhoanId">Id tài khoản, lấy từ token.</param>
    Task<QuyenHanDto> BLLayQuyenHanAsync(Guid taiKhoanId, CancellationToken cancellationToken);
}
