using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/06-huan-luyen.md. Mục 3.
/// <summary>
/// Gộp nhật ký bữa ăn và báo cáo buổi tập. PT nhận xét và duyệt; mỗi lần sửa hoặc đánh giá lại ghi vào
/// lich_su_chinh_sua.
/// </summary>
public sealed class nhat_ky_hoat_dong : BaseAuditableEntity
{
    /// <summary>Phân quyền theo kết nối PT; null nếu học viên tự ghi riêng.</summary>
    public Guid? ket_noi_pt_id { get; set; }

    /// <summary>Mục lịch làm căn cứ; null nếu hoạt động phát sinh.</summary>
    public Guid? muc_ke_hoach_id { get; set; }

    /// <summary>Buổi tập liên quan khi là nhật ký tập.</summary>
    public Guid? buoi_tap_id { get; set; }

    /// <summary>Học viên sở hữu dữ liệu.</summary>
    public Guid hoc_vien_id { get; set; }

    /// <summary>Phiên bản kế hoạch lúc nộp; null nếu hoạt động phát sinh. Đề xuất, chưa duyệt.</summary>
    public int? phien_ban_ke_hoach { get; set; }

    public LoaiHoatDong loai_hoat_dong { get; set; }

    /// <summary>Ngày ăn hoặc tập theo múi giờ người dùng.</summary>
    public DateOnly ngay_hoat_dong { get; set; }

    public DateTime thoi_gian_thuc_hien { get; set; }

    public string? noi_dung { get; set; }

    /// <summary>JSON: khẩu phần, khối lượng hoặc bài tập thực tế.</summary>
    public string? chi_tiet { get; set; }

    public TrangThaiNhatKy trang_thai { get; set; }

    /// <summary>PT chịu trách nhiệm phản hồi.</summary>
    public Guid? nguoi_danh_gia_id { get; set; }

    /// <summary>Kết quả phản hồi mới nhất.</summary>
    public string? nhan_xet { get; set; }

    public DateTime? ngay_danh_gia { get; set; }
}
