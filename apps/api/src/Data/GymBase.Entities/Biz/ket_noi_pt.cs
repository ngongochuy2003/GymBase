using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Gộp yêu cầu thuê PT và quan hệ huấn luyện. Mỗi lần hợp tác là một dòng. Là gốc phân quyền: PT chỉ quản lý dữ
/// liệu thuộc kết nối đang hoạt động.
/// </summary>
public sealed class ket_noi_pt : BaseEntity
{
    public Guid pt_id { get; set; }

    public Guid hoc_vien_id { get; set; }

    /// <summary>Người gửi lời mời.</summary>
    public Guid nguoi_khoi_tao_id { get; set; }

    /// <summary>Lĩnh vực PT phụ trách, lưu dạng JSON; có thể cả hai.</summary>
    public List<PhamViPhuTrach> pham_vi_phu_trach { get; set; } = [];

    public string? loi_nhan { get; set; }

    public TrangThaiKetNoiPt trang_thai { get; set; }

    public DateTime ngay_de_nghi { get; set; }

    public DateTime? ngay_chap_nhan { get; set; }

    public DateTime? ngay_ket_thuc { get; set; }

    /// <summary>Lý do từ chối, hủy hoặc kết thúc.</summary>
    public string? ly_do_ket_thuc { get; set; }
}
