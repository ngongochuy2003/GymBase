using GymBase.Base.Auth;
using GymBase.Base.Hosting;
using GymBase.BLAuth.TaiKhoan;
using Microsoft.AspNetCore.Authorization;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// API quản lý người dùng trên kênh mobile: /api/v1/mob/users, chỉ Admin, dùng chung
/// <see cref="UserService"/> với web. Học viên và PT xem thông tin của mình qua /auth/me.
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController(UserService userService)
    : BaseCrudController<TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto>(userService);
