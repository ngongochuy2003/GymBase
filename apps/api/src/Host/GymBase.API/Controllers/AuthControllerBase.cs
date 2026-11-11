using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Auth;
using GymBase.BLAuth.TaiKhoan;
using GymBase.BLAuth.XacThuc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymBase.API.Controllers;

/// <summary>Các endpoint đăng nhập, dùng chung cho web và mobile; kênh do lớp con quyết định theo namespace.</summary>
public abstract class AuthControllerBase(AuthService authService) : BaseApiController
{
    /// <summary>Đăng nhập; 401 nếu không thành công, không nói rõ lý do.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<ActionResult<KetQuaDangNhapDto>> Login(DangNhapDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.LoginAsync(dto, cancellationToken));

    /// <summary>Đổi refresh token lấy cặp token mới; refresh token cũ không dùng lại được.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<ActionResult<KetQuaDangNhapDto>> Refresh(LamMoiTokenDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.RefreshAsync(dto, cancellationToken));

    /// <summary>Đăng xuất khỏi phiên hiện tại.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Thông tin của người đang đăng nhập.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<TaiKhoanDto>> Me(CancellationToken cancellationToken) =>
        Ok(await authService.GetCurrentUserAsync(cancellationToken));

    /// <summary>Đổi mật khẩu của chính mình; các phiên khác bị thu hồi.</summary>
    [HttpPost("change-password")]
    [EnableRateLimiting(AuthenticationExtensions.AuthRateLimitPolicy)]
    public async Task<IActionResult> ChangePassword(DoiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(dto, cancellationToken);
        return NoContent();
    }
}

/// <summary>Đăng nhập cho web: /api/v1/web/auth.</summary>
public sealed class AuthController(AuthService authService) : AuthControllerBase(authService);
