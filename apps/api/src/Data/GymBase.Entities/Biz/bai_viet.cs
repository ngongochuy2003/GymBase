using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/03-mang-xa-hoi.md. Mục 3.
/// <summary>
/// Bài đăng văn bản, ảnh hoặc video. Bảng tin và feed video dùng chung. File đính kèm gắn qua tep_tin.
/// </summary>
public sealed class bai_viet : BaseAuditableEntity
{
    public Guid tac_gia_id { get; set; }

    /// <summary>Có thể chỉ đăng ảnh hoặc video.</summary>
    public string? noi_dung { get; set; }

    public LoaiBaiViet loai_bai_viet { get; set; }

    public CheDoHienThi che_do_hien_thi { get; set; }

    public TrangThaiBaiViet trang_thai { get; set; }

    /// <summary>Lúc công khai (UTC).</summary>
    public DateTime? ngay_dang { get; set; }
}
