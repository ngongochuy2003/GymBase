using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/06-huan-luyen.md. Mục 3.
/// <summary>
/// Kế hoạch dinh dưỡng hoặc tập luyện, áp dụng trong một kết nối PT.
/// </summary>
public sealed class ke_hoach : BaseAuditableEntity
{
    public Guid ket_noi_pt_id { get; set; }

    public LoaiKeHoach loai_ke_hoach { get; set; }

    public string ten_ke_hoach { get; set; } = string.Empty;

    public string? mo_ta { get; set; }

    public DateOnly ngay_bat_dau { get; set; }

    /// <summary>Phải không nhỏ hơn ngay_bat_dau.</summary>
    public DateOnly? ngay_ket_thuc { get; set; }

    /// <summary>Tăng khi sửa nội dung đã áp dụng.</summary>
    public int phien_ban { get; set; }

    public TrangThaiKeHoach trang_thai { get; set; }

    /// <summary>Thường là PT phụ trách. Khác với nguoi_tao (tên đăng nhập) của audit.</summary>
    public Guid nguoi_tao_id { get; set; }
}
