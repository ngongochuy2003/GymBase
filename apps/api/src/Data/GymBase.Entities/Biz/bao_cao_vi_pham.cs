using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/09-an-toan.md. Mục 3.
/// <summary>
/// Người dùng báo cáo bài đăng, bình luận, tài khoản hoặc tin nhắn.
/// </summary>
public sealed class bao_cao_vi_pham : BaseEntity
{
    public Guid nguoi_bao_cao_id { get; set; }

    public LoaiDoiTuongBaoCao loai_doi_tuong { get; set; }

    /// <summary>Đa hình, không có khóa ngoại.</summary>
    public Guid doi_tuong_id { get; set; }

    public LyDoBaoCao ly_do { get; set; }

    public string? mo_ta { get; set; }

    public TrangThaiBaoCao trang_thai { get; set; }

    /// <summary>Quản trị viên xử lý.</summary>
    public Guid? nguoi_xu_ly_id { get; set; }

    public DateTime ngay_tao { get; set; }

    public DateTime? ngay_xu_ly { get; set; }
}
