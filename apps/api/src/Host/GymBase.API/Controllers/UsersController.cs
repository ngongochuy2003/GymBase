using GymBase.Base.Auth;
using GymBase.Base.Hosting;
using GymBase.BLAuth.NguoiDung;
using GymBase.BLAuth.XacThuc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Controllers;

/// <summary>
/// API quản lý người dùng cho web (trang quản trị): /api/v1/web/users, chỉ Admin. Đủ CRUD từ
/// <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>: GET danh sách, GET {id}, POST, PUT {id}, DELETE {id} (xóa mềm).
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController(UserService userService, AuthService authService)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userService)
{
    /// <summary>Đặt lại mật khẩu cho người dùng; mọi phiên của người đó bị thu hồi.</summary>
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, DatLaiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(id, dto, cancellationToken);
        return NoContent();
    }
}
