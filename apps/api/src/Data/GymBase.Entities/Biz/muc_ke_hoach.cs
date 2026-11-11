using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/06-huan-luyen.md. Mục 3.
/// <summary>
/// Gộp lịch bữa ăn và lịch tập. Chi tiết riêng lưu JSON, kiểm tra theo schema của từng loại.
/// </summary>
public sealed class muc_ke_hoach : BaseAuditableEntity
{
    public Guid ke_hoach_id { get; set; }

    public LoaiMuc loai_muc { get; set; }

    public string tieu_de { get; set; } = string.Empty;

    /// <summary>Giờ địa phương của kế hoạch.</summary>
    public TimeOnly gio_du_kien { get; set; }

    /// <summary>Các thứ lặp lại, số 1 đến 7, lưu dạng JSON.</summary>
    public List<int> cac_thu_trong_tuan { get; set; } = [];

    /// <summary>Múi giờ IANA, ví dụ Asia/Ho_Chi_Minh.</summary>
    public string mui_gio { get; set; } = string.Empty;

    /// <summary>JSON: khẩu phần dự kiến hoặc danh sách bài tập.</summary>
    public string chi_tiet { get; set; } = "{}";

    public bool co_nhac { get; set; }

    /// <summary>0 là đúng giờ.</summary>
    public int nhac_truoc_phut { get; set; }

    public TrangThaiMucKeHoach trang_thai { get; set; }
}
