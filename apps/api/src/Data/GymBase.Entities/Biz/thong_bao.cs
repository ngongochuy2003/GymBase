using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/08-thong-bao.md. Mục 3.
/// <summary>
/// Hộp thông báo và hàng đợi logic cho nhắc ăn, nhắc tập.
/// </summary>
public sealed class thong_bao : BaseEntity
{
    /// <summary>Người nhận.</summary>
    public Guid tai_khoan_id { get; set; }

    public LoaiThongBao loai_thong_bao { get; set; }

    public string tieu_de { get; set; } = string.Empty;

    public string? noi_dung { get; set; }

    /// <summary>Nguồn phát sinh, ví dụ muc_ke_hoach, tin_nhan. Danh sách chưa chốt.</summary>
    public string? loai_doi_tuong { get; set; }

    /// <summary>Đa hình, không có khóa ngoại.</summary>
    public Guid? doi_tuong_id { get; set; }

    /// <summary>Thời điểm cần gửi (UTC).</summary>
    public DateTime thoi_diem_du_kien { get; set; }

    /// <summary>Lúc yêu cầu push được dispatch (UTC).</summary>
    public DateTime? thoi_diem_da_gui { get; set; }

    public TrangThaiGui trang_thai_gui { get; set; }

    public DateTime? ngay_da_doc { get; set; }

    /// <summary>Khóa ngăn lập trùng hoặc retry trùng. Unique cùng tai_khoan_id.</summary>
    public string ma_chong_trung { get; set; } = string.Empty;

    public DateTime ngay_tao { get; set; }
}
