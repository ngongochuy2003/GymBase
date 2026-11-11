using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/03-mang-xa-hoi.md. Mục 3.
/// <summary>
/// Gộp lượt thích và bình luận. Bình luận có thể trả lời bình luận khác cùng bài.
/// </summary>
public sealed class tuong_tac_bai_viet : BaseEntity
{
    public Guid bai_viet_id { get; set; }

    /// <summary>Người thực hiện.</summary>
    public Guid tai_khoan_id { get; set; }

    public LoaiTuongTac loai_tuong_tac { get; set; }

    /// <summary>Bắt buộc với binh_luan, null với thich.</summary>
    public string? noi_dung { get; set; }

    /// <summary>Bình luận cha; chỉ trỏ tới bình luận cùng bài, không trỏ tới lượt thích.</summary>
    public Guid? tuong_tac_cha_id { get; set; }

    public DateTime ngay_tao { get; set; }

    /// <summary>Xóa mềm. Entity không có ISoftDelete nên query filter không tự lọc, service phải lọc.</summary>
    public DateTime? ngay_xoa { get; set; }
}
