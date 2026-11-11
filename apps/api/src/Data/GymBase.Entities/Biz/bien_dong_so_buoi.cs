using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/07-goi-pt-buoi-tap.md. Mục 3.
/// <summary>
/// Sổ cái cộng, trừ, hoàn số buổi PT. Bất biến: không UPDATE hay DELETE, điều chỉnh bằng giao dịch bù.
/// </summary>
public sealed class bien_dong_so_buoi : BaseEntity
{
    public Guid goi_pt_hoc_vien_id { get; set; }

    /// <summary>Buổi liên quan; null cho lần cấp buổi ban đầu.</summary>
    public Guid? buoi_tap_id { get; set; }

    public LoaiBienDong loai_bien_dong { get; set; }

    /// <summary>Số nguyên có dấu, khác 0: +12, -1, +1.</summary>
    public int so_buoi_thay_doi { get; set; }

    /// <summary>Khóa chống trùng cho sự kiện phát sinh. Unique.</summary>
    public string ma_chong_trung { get; set; } = string.Empty;

    public Guid nguoi_tao_id { get; set; }

    public string? ly_do { get; set; }

    public DateTime ngay_tao { get; set; }
}
