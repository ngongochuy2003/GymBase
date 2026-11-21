using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>Thông tin và trạng thái onboarding của người đang đăng nhập (<c>/auth/me</c>).</summary>
public sealed class TaiKhoanHienTaiService(ITaiKhoanRepository users, ICurrentUser currentUser, TimeProvider timeProvider)
{
    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task<TaiKhoanDto> BLLayTaiKhoanHienTaiAsync(CancellationToken cancellationToken) =>
        TaiKhoanMapper.ToDto(await GetEntityAsync(cancellationToken));

    /// <summary>
    /// Người dùng đã bấm một trong hai nút của màn onboarding: đánh dấu <c>is_dang_nhap_lan_dau = false</c>. Gọi lại nhiều
    /// lần vẫn an toàn.
    /// </summary>
    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task<TaiKhoanDto> BLHoanTatDangNhapLanDauAsync(CancellationToken cancellationToken)
    {
        var user = await GetEntityAsync(cancellationToken);
        if (user.is_dang_nhap_lan_dau)
        {
            user.is_dang_nhap_lan_dau = false;
            user.MarkUpdated(currentUser.UserName, timeProvider.GetUtcNow().UtcDateTime);
            await users.DLUpdateAsync(user, cancellationToken);
        }

        return TaiKhoanMapper.ToDto(user);
    }

    private async Task<tai_khoan> GetEntityAsync(CancellationToken cancellationToken)
    {
        var user = currentUser.UserId is { } id ? await users.DLGetByIdAsync(id, cancellationToken) : null;
        return user ?? throw new UnauthorizedException();
    }
}
