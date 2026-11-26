using GymBase.Base.Crud;
using GymBase.Entities.DI;

namespace GymBase.DLDI.Contract.DLDanhMuc;

/// <summary>
/// Truy cập bảng <c>danh_muc</c>. CRUD lấy từ <see cref="IBaseRepository{TEntity}"/>; truy vấn riêng của
/// danh mục thêm vào đây.
/// </summary>
public interface IDanhMucRepository : IBaseRepository<danh_muc>
{
    /// <summary>
    /// Các mục đang dùng (<c>tinh_trang = 1</c>), sắp theo loại rồi theo tên. <paramref name="maDinhDanhs"/> null hoặc
    /// rỗng thì lấy mọi loại. <paramref name="maCapTren"/> null thì chỉ lấy mục gốc; có giá trị thì lấy mục con trực tiếp
    /// của mục mang mã đó trong cùng loại.
    /// </summary>
    Task<IReadOnlyList<danh_muc>> DLFindDangDungAsync(
        IReadOnlyCollection<string>? maDinhDanhs, string? maCapTren, CancellationToken cancellationToken);

    /// <summary>True nếu có mục đang dùng mang cặp (loại, mã), ở bất kỳ cấp nào.</summary>
    Task<bool> DLExistsByMaAsync(string maDinhDanh, string ma, CancellationToken cancellationToken);
}
