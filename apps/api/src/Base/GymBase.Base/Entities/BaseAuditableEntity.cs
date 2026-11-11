namespace GymBase.Base.Entities;

/// <summary>
/// Entity có thông tin ai tạo, ai sửa và lúc nào. Dùng cho danh mục và bảng nghiệp vụ được người dùng sửa.
/// Mọi mốc thời gian lưu ở UTC; chỉ đổi sang giờ địa phương khi hiển thị.
/// Không gán trực tiếp các property này, dùng <see cref="EntityAuditExtensions"/>.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>Thời điểm tạo (UTC).</summary>
    public DateTime ngay_tao { get; set; }

    /// <summary>Người tạo; null nếu do hệ thống (job, seed) tạo.</summary>
    public string? nguoi_tao { get; set; }

    /// <summary>Thời điểm sửa gần nhất (UTC); null nếu chưa sửa lần nào.</summary>
    public DateTime? ngay_chinh_sua { get; set; }

    /// <summary>Người sửa gần nhất.</summary>
    public string? nguoi_chinh_sua { get; set; }
}
