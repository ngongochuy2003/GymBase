using GymBase.Base.Auth;
using GymBase.DLAuth.Contract.DLTaiKhoan;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Cho nhóm khác lấy tên hiển thị của tài khoản mà không phải đọc entity của nhóm Auth.</summary>
public sealed class TaiKhoanHoTenService(ITaiKhoanRepository taiKhoans) : ITaiKhoanHoTenProvider
{
    public async Task<string?> BLDocHoTenAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        (await taiKhoans.DLGetByIdAsync(taiKhoanId, cancellationToken))?.ho_ten;
}
