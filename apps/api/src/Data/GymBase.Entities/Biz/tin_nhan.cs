using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/05-nhan-tin.md. Mục 3.
/// <summary>
/// Tin nhắn văn bản hoặc kèm file (qua tep_tin).
/// </summary>
public sealed class tin_nhan : BaseEntity
{
    public Guid cuoc_tro_chuyen_id { get; set; }

    public Guid nguoi_gui_id { get; set; }

    /// <summary>Khóa chống gửi trùng do client sinh. Unique cùng nguoi_gui_id.</summary>
    public Guid ma_tin_nhan_client { get; set; }

    public LoaiTinNhan loai_tin_nhan { get; set; }

    public string? noi_dung { get; set; }

    /// <summary>Tin nhắn được trả lời.</summary>
    public Guid? tra_loi_tin_nhan_id { get; set; }

    /// <summary>Lúc server ghi nhận (UTC).</summary>
    public DateTime ngay_gui { get; set; }

    /// <summary>Ẩn hoặc xóa theo nghiệp vụ. Entity không có ISoftDelete nên service phải lọc.</summary>
    public DateTime? ngay_xoa { get; set; }
}
