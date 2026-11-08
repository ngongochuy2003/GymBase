using GymBase.AMISBase.API;
using GymBase.BLSystem.NguoiDung;

namespace GymBase.SystemAPI.Controllers;

/// <summary>
/// API người dùng cho web (trang quản trị): /api/web/v1/users. Đủ CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>:
/// GET danh sách, GET {id}, POST, PUT {id}, DELETE {id} (xóa mềm).
/// </summary>
public sealed class UsersController(UserBL userBL)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userBL);
