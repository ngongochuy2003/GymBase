using GymBase.Base.Auth;
using GymBase.Base.Hosting;
using GymBase.BLAuth.BLTaiKhoan;
using Microsoft.AspNetCore.Authorization;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// API quản lý người dùng trên kênh mobile: /api/v1/mob/users, chỉ Admin, dùng chung
/// <see cref="TaiKhoanService"/> với web. Học viên và PT xem thông tin của mình qua /auth/me.
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController(TaiKhoanService userService)
    : BaseCrudController<TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto>(userService);
