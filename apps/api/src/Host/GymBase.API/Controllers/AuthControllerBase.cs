using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Auth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLAuth.BLXacThuc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymBase.API.Controllers;

/// <summary>Các endpoint đăng nhập, dùng chung cho web và mobile; kênh do lớp con quyết định theo namespace.</summary>
public abstract class AuthControllerBase(XacThucService authService, IQuyenHanProvider quyenHanProvider, ICurrentUser currentUser)
    : BaseApiController
{
    /// <summary>Đăng nhập; 401 nếu không thành công, không nói rõ lý do.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<ActionResult<KetQuaDangNhapDto>> Login(DangNhapDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.BLDangNhapAsync(dto, cancellationToken));

    /// <summary>Đổi refresh token lấy cặp token mới; refresh token cũ không dùng lại được.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<ActionResult<KetQuaDangNhapDto>> Refresh(LamMoiTokenDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.BLLamMoiTokenAsync(dto, cancellationToken));

    /// <summary>Đăng xuất khỏi phiên hiện tại.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authService.BLDangXuatAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Thông tin của người đang đăng nhập.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<TaiKhoanDto>> Me(CancellationToken cancellationToken) =>
        Ok(await authService.BLLayTaiKhoanHienTaiAsync(cancellationToken));

    /// <summary>
    /// Quyền theo hồ sơ của người đang đăng nhập (nhóm, trạng thái xác minh PT, năng lực), tính từ database mỗi lần gọi.
    /// Client dùng để hiện hoặc ẩn chức năng; backend vẫn kiểm tra lại ở từng request.
    /// </summary>
    [HttpGet("quyen-han")]
    public async Task<ActionResult<QuyenHanDto>> QuyenHan(CancellationToken cancellationToken) =>
        Ok(await quyenHanProvider.BLLayQuyenHanAsync(currentUser.UserId ?? throw new UnauthorizedException(), cancellationToken));

    /// <summary>Đổi mật khẩu của chính mình; các phiên khác bị thu hồi.</summary>
    [HttpPost("change-password")]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<IActionResult> ChangePassword(DoiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await authService.BLDoiMatKhauAsync(dto, cancellationToken);
        return NoContent();
    }
}

/// <summary>Đăng nhập cho web: /api/v1/web/auth.</summary>
public sealed class AuthController(XacThucService authService, IQuyenHanProvider quyenHanProvider, ICurrentUser currentUser)
    : AuthControllerBase(authService, quyenHanProvider, currentUser);
