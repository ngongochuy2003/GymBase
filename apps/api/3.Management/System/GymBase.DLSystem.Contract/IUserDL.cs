using GymBase.AMISBase.DL.Contract;
using GymBase.Model.Entities;

namespace GymBase.DLSystem.Contract;

/// <summary>
/// Truy cập dữ liệu người dùng. CRUD lấy từ <see cref="IBaseDL{TEntity}"/>; tìm kiếm khớp
/// tên đăng nhập, họ tên, email, số điện thoại. Truy vấn riêng của người dùng thêm vào đây.
/// </summary>
public interface IUserDL : IBaseDL<nguoi_dung>
{
}
