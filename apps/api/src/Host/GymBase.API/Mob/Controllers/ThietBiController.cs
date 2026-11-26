using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.BLThongBao.BLThietBi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Thiết bị và push token trên kênh mobile: <c>/api/v1/mob/thiet-bi</c>. App báo thiết bị khi đăng nhập, mở app và khi token đổi;
/// gỡ thiết bị trước khi đăng xuất.
/// </summary>
[Authorize]
[Route("thiet-bi")]
public sealed class ThietBiController(ThietBiService thietBiService, ICurrentUser currentUser) : BaseApiController
{
    private Guid TaiKhoanId => currentUser.UserId ?? throw new UnauthorizedException();

    /// <summary>Đăng ký hoặc cập nhật thiết bị (upsert theo mã thiết bị).</summary>
    [HttpPut]
    public async Task<IActionResult> DangKy([FromBody] DangKyThietBiDto dto, CancellationToken ct)
    {
        await thietBiService.BLDangKyAsync(TaiKhoanId, dto, ct);
        return NoContent();
    }

    /// <summary>Gỡ token và tắt thiết bị, gọi trước khi đăng xuất để người dùng kế tiếp trên máy không nhận thông báo của người trước.</summary>
    [HttpDelete("{maThietBi}")]
    public async Task<IActionResult> Go(string maThietBi, CancellationToken ct)
    {
        await thietBiService.BLGoAsync(TaiKhoanId, maThietBi, ct);
        return NoContent();
    }
}
