using GymBase.AMISBase.API;
using GymBase.BLSystem.NguoiDung;

namespace GymBase.MobAPI.Controllers;

/// <summary>
/// API người dùng cho app mobile: /api/mobile/v1/users. Đủ CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>,
/// dùng chung <see cref="UserBL"/> với web.
/// </summary>
public sealed class UsersController(UserBL userBL)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userBL);
