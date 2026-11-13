using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Gói buổi PT đã thỏa thuận giữa một PT và học viên. Số buổi còn lại không lưu, tính bằng
/// SUM(bien_dong_so_buoi.so_buoi_thay_doi).
/// </summary>
public sealed class goi_pt_hoc_vien : BaseAuditableEntity
{
    public Guid ket_noi_pt_id { get; set; }

    /// <summary>Tên gói tại thời điểm thỏa thuận.</summary>
    public string ten_goi { get; set; } = string.Empty;

    /// <summary>Lớn hơn 0.</summary>
    public int so_buoi_mua { get; set; }

    /// <summary>Chỉ để tham khảo, chưa xử lý thanh toán.</summary>
    public decimal? gia_thoa_thuan { get; set; }

    public DateOnly ngay_bat_dau { get; set; }

    public DateOnly? ngay_het_han { get; set; }

    public TrangThaiGoiPt trang_thai { get; set; }

    /// <summary>PT hoặc người có quyền tạo. Khác với nguoi_tao (tên đăng nhập) của audit.</summary>
    public Guid nguoi_tao_id { get; set; }
}
