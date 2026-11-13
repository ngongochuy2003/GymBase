using GymBase.Base.Auth;
using GymBase.DLAuth.Contract.DLTaiKhoan;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Cho nhóm khác biết tài khoản còn hoạt động không, để nhóm đó không phải đọc entity của nhóm Auth.</summary>
public sealed class TaiKhoanTrangThaiService(ITaiKhoanRepository taiKhoans) : ITaiKhoanTrangThaiProvider
{
    public async Task<bool> BLKiemTraHoatDongAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        await taiKhoans.DLGetByIdAsync(taiKhoanId, cancellationToken) is { dang_hoat_dong: true };
}
