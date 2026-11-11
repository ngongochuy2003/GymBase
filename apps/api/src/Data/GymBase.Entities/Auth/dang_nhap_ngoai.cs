using GymBase.Base.Entities;
using GymBase.Entities.Auth.Enums;

namespace GymBase.Entities.Auth;

// TODO(DB): chưa có migration (InitialBase); sơ đồ ở database/diagrams/01-tai-khoan-ho-so.md. Mục 3.
/// <summary>
/// Liên kết một tài khoản với tài khoản đăng nhập ngoài (Google). Định danh bằng mã của nhà cung cấp, không
/// dùng email.
/// </summary>
public sealed class dang_nhap_ngoai : BaseEntity
{
    public Guid tai_khoan_id { get; set; }

    public NhaCungCap nha_cung_cap { get; set; }

    /// <summary>Mã người dùng do nhà cung cấp cấp (sub của Google). Unique cùng nha_cung_cap.</summary>
    public string ma_ngoai { get; set; } = string.Empty;

    /// <summary>Lúc liên kết (UTC).</summary>
    public DateTime ngay_tao { get; set; }
}
