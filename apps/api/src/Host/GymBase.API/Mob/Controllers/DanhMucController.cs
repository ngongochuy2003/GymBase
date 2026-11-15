using GymBase.Base.Hosting;
using GymBase.BLDI.BLDanhMuc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Danh mục cho app mobile: <c>/api/v1/mob/danh-muc</c>. App đọc trước khi đổ dropdown hồ sơ; cần đăng nhập.
/// </summary>
[Authorize]
[Route("danh-muc")]
public sealed class DanhMucController(DanhMucService danhMucService) : BaseApiController
{
    /// <summary>
    /// Danh mục đang dùng, nhóm theo loại. Truyền nhiều loại bằng dấu phẩy, ví dụ
    /// <c>?ma_dinh_danh=muc_tieu_tap_luyen,khu_vuc</c>; bỏ trống thì trả mọi loại.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DanhMucNhomDto>>> LayDanhSach(
        [FromQuery] string? ma_dinh_danh, CancellationToken cancellationToken) =>
        Ok(await danhMucService.BLLayDanhMucAsync(ChiaMaDinhDanh(ma_dinh_danh), cancellationToken));

    private static IReadOnlyCollection<string>? ChiaMaDinhDanh(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var maDinhDanhs = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return maDinhDanhs.Count == 0 ? null : maDinhDanhs;
    }
}
