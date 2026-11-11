using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/07-goi-pt-buoi-tap.md. Mục 3.
/// <summary>
/// Buổi tập cụ thể. Chỉ buổi pt_truc_tiep do đúng PT xác nhận mới trừ gói; tự tập không bao giờ bị trừ. Khiếu
/// nại nằm ngay trong bảng, mỗi buổi chỉ một khiếu nại đang xử lý.
/// </summary>
public sealed class buoi_tap : BaseAuditableEntity
{
    /// <summary>Null khi học viên tự tập độc lập.</summary>
    public Guid? ket_noi_pt_id { get; set; }

    /// <summary>Gói dự kiến trừ nếu là PT trực tiếp.</summary>
    public Guid? goi_pt_hoc_vien_id { get; set; }

    /// <summary>Mục lịch tập gốc nếu có.</summary>
    public Guid? muc_ke_hoach_id { get; set; }

    public Guid hoc_vien_id { get; set; }

    /// <summary>PT phụ trách khi có PT trực tiếp.</summary>
    public Guid? pt_id { get; set; }

    public LoaiBuoi loai_buoi { get; set; }

    public DateTime? bat_dau_du_kien { get; set; }

    public DateTime? bat_dau_thuc_te { get; set; }

    public DateTime? ket_thuc_thuc_te { get; set; }

    public TrangThaiBuoiTap trang_thai { get; set; }

    /// <summary>PT xác nhận buổi tính phí.</summary>
    public Guid? nguoi_xac_nhan_id { get; set; }

    public DateTime? ngay_xac_nhan { get; set; }

    /// <summary>Khiếu nại gần nhất.</summary>
    public string? noi_dung_khieu_nai { get; set; }

    public TrangThaiKhieuNai? trang_thai_khieu_nai { get; set; }

    /// <summary>Lúc học viên gửi khiếu nại. Đề xuất, chưa duyệt.</summary>
    public DateTime? ngay_khieu_nai { get; set; }

    /// <summary>Ai quyết định chấp nhận hoặc bác. Đề xuất, chưa duyệt.</summary>
    public Guid? nguoi_xu_ly_khieu_nai_id { get; set; }

    /// <summary>Đề xuất, chưa duyệt.</summary>
    public DateTime? ngay_xu_ly_khieu_nai { get; set; }
}
