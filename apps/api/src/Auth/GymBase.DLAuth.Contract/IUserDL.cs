using GymBase.Base.Crud;
using GymBase.DLAuth.Contract.Entities;

namespace GymBase.DLAuth.Contract;

/// <summary>
/// Truy cập dữ liệu người dùng. CRUD lấy từ <see cref="IBaseDL{TEntity}"/>; tìm kiếm khớp
/// tên đăng nhập, họ tên, email, số điện thoại. Truy vấn riêng của người dùng thêm vào đây.
/// </summary>
public interface IUserDL : IBaseDL<nguoi_dung>
{
    /// <summary>
    /// Đã có người dùng còn hiệu lực (chưa xóa) với tên đăng nhập này chưa; so khớp không phân biệt
    /// hoa thường, giống collation mặc định của MySQL.
    /// </summary>
    Task<bool> ExistsByTaiKhoanAsync(string taiKhoan, CancellationToken cancellationToken);
}
