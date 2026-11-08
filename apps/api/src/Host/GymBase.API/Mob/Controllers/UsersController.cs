using GymBase.Base.Hosting;
using GymBase.BLAuth.NguoiDung;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// API người dùng cho app mobile: /api/v1/mob/users. Đủ CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>,
/// dùng chung <see cref="UserBL"/> với web.
/// </summary>
public sealed class UsersController(UserBL userBL)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userBL);
