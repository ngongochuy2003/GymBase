using GymBase.Base.Crud;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;

namespace GymBase.DLAuth.Contract.DLDangNhapNgoai;

/// <summary>Truy cập liên kết giữa tài khoản và tài khoản đăng nhập ngoài (Google).</summary>
public interface IDangNhapNgoaiRepository : IBaseRepository<dang_nhap_ngoai>
{
    /// <summary>Liên kết theo mã do nhà cung cấp cấp (sub của Google); null nếu chưa có.</summary>
    Task<dang_nhap_ngoai?> DLFindByMaNgoaiAsync(NhaCungCap nhaCungCap, string maNgoai, CancellationToken cancellationToken);
}
