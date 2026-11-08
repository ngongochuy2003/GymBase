using GymBase.Base.Hosting;
using GymBase.BLAuth.NguoiDung;

namespace GymBase.API.Controllers;

/// <summary>
/// API người dùng cho web (trang quản trị): /api/v1/web/users. Đủ CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/>:
/// GET danh sách, GET {id}, POST, PUT {id}, DELETE {id} (xóa mềm).
/// </summary>
public sealed class UsersController(UserService userService)
    : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userService);
