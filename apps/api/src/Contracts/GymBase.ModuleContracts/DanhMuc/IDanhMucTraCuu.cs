namespace GymBase.ModuleContracts.DanhMuc;

/// <summary>
/// Tra cứu danh mục cho nhóm khác (BLBiz cần kiểm tra mã trước khi lưu hồ sơ) mà không phải tham chiếu BLDI.
/// Hiện thực nằm ở BLDI và được host đăng ký.
/// </summary>
public interface IDanhMucTraCuu
{
    /// <summary>True nếu cặp (<paramref name="maDinhDanh"/>, <paramref name="ma"/>) tồn tại và đang dùng (<c>tinh_trang = 1</c>).</summary>
    Task<bool> BLKiemTraDangDungAsync(string maDinhDanh, string ma, CancellationToken cancellationToken);
}
