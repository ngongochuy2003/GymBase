using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Hồ sơ duy nhất của mỗi tài khoản. Khóa chính là tai_khoan_id nên không kế thừa BaseEntity và không dùng CRUD
/// chung; bốn cột audit khai báo và ghi bằng tay. Tài khoản chưa chọn nhóm thì chưa có dòng nào.
/// </summary>
public sealed class ho_so_nguoi_dung
{
    /// <summary>Khóa chính kiêm khóa ngoại tới tai_khoan (1-1).</summary>
    public Guid tai_khoan_id { get; set; }

    public LoaiNguoiDung loai_nguoi_dung { get; set; }

    /// <summary>Tên công khai trên hồ sơ và bài viết.</summary>
    public string ten_hien_thi { get; set; } = string.Empty;

    public string? gioi_thieu { get; set; }

    /// <summary>Ảnh đại diện, trỏ tới tep_tin.</summary>
    public Guid? anh_dai_dien_tep_tin_id { get; set; }

    /// <summary>Mục tiêu cá nhân của học viên, mặc định riêng tư.</summary>
    public string? muc_tieu_tap_luyen { get; set; }

    /// <summary>Mã chuyên môn của PT, lưu dạng JSON.</summary>
    public List<string>? chuyen_mon { get; set; }

    public short? so_nam_kinh_nghiem { get; set; }

    /// <summary>Quận, huyện hoặc địa bàn PT công khai.</summary>
    public string? khu_vuc_hoat_dong { get; set; }

    /// <summary>Vị trí phục vụ xấp xỉ, không phải tọa độ nhà ở.</summary>
    public decimal? vi_do { get; set; }

    public decimal? kinh_do { get; set; }

    public short? ban_kinh_phuc_vu_km { get; set; }

    public TrangThaiXacMinh trang_thai_xac_minh { get; set; }

    /// <summary>Lần Admin duyệt hoặc từ chối gần nhất (UTC).</summary>
    public DateTime? ngay_xac_minh { get; set; }

    /// <summary>Admin xử lý.</summary>
    public Guid? nguoi_xac_minh_id { get; set; }

    /// <summary>Chỉ có khi trang_thai_xac_minh là tu_choi.</summary>
    public string? ly_do_tu_choi { get; set; }

    /// <summary>Thời điểm tạo (UTC).</summary>
    public DateTime ngay_tao { get; set; }

    /// <summary>Người tạo, ghi tên đăng nhập; null nếu do hệ thống tạo.</summary>
    public string? nguoi_tao { get; set; }

    /// <summary>Thời điểm sửa gần nhất (UTC); null nếu chưa sửa.</summary>
    public DateTime? ngay_chinh_sua { get; set; }

    /// <summary>Người sửa gần nhất.</summary>
    public string? nguoi_chinh_sua { get; set; }
}
