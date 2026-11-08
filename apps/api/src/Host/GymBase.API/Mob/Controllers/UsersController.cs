using GymBase.Base.Hosting;
using GymBase.BLAuth.NguoiDung;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// API người dùng cho app mobile: /api/v1/mob/users. Đủ CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>,
/// dùng chung <see cref="UserService"/> với web.
/// </summary>
public sealed class UsersController(UserService userService)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userService);
