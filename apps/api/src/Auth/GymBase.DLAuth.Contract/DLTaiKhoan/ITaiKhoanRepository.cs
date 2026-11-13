using GymBase.Base.Crud;
using GymBase.Entities.Auth;

namespace GymBase.DLAuth.Contract.DLTaiKhoan;

/// <summary>
/// Truy cập dữ liệu người dùng. CRUD lấy từ <see cref="IBaseRepository{TEntity}"/>; tìm kiếm khớp
/// tên đăng nhập, họ tên, email, số điện thoại. Truy vấn riêng của người dùng thêm vào đây.
/// </summary>
public interface ITaiKhoanRepository : IBaseRepository<tai_khoan>
{
    /// <summary>
    /// Đã có người dùng còn hiệu lực (chưa xóa) với tên đăng nhập này chưa; so khớp không phân biệt
    /// hoa thường, giống collation mặc định của MySQL.
    /// </summary>
    Task<bool> DLExistsByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken);

    /// <summary>
    /// Đã có người dùng khác (không phải <paramref name="exceptId"/>) dùng email này chưa. Email đã chuẩn hóa chữ thường.
    /// </summary>
    Task<bool> DLExistsByEmailAsync(string email, Guid? exceptId, CancellationToken cancellationToken);

    /// <summary>Người dùng theo email đã chuẩn hóa chữ thường; null nếu không có.</summary>
    Task<tai_khoan?> DLFindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Người dùng còn hiệu lực theo tên đăng nhập, không phân biệt hoa thường; null nếu không có.</summary>
    Task<tai_khoan?> DLFindByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken);
}
