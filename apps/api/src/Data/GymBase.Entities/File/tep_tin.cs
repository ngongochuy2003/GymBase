using GymBase.Base.Entities;

namespace GymBase.Entities.File;

/// <summary>
/// Tệp tin lưu trữ (Local Storage / Amazon S3).
/// Tên class trùng tên bảng <c>tep_tin</c>, property trùng tên cột.
/// </summary>
public sealed class tep_tin : BaseEntity
{
    /// <summary>Người tải lên tệp tin.</summary>
    public Guid nguoi_tai_len_id { get; set; }

    /// <summary>Tên tệp tin gốc từ thiết bị client.</summary>
    public string ten_tep_goc { get; set; } = string.Empty;

    /// <summary>Khóa lưu trữ (object key / file path) duy nhất.</summary>
    public string khoa_luu_tru { get; set; } = string.Empty;

    /// <summary>Nhà cung cấp lưu trữ: 'LOCAL' hoặc 'S3'.</summary>
    public string nha_cung_cap { get; set; } = "LOCAL";

    /// <summary>Loại MIME chuẩn, ví dụ 'image/webp', 'image/jpeg'.</summary>
    public string loai_mime { get; set; } = string.Empty;

    /// <summary>Dung lượng tệp tin tính theo byte.</summary>
    public long dung_luong { get; set; }

    /// <summary>Mục đích tệp tin: avatar, gym_photo, progress, post.</summary>
    public string muc_dich { get; set; } = string.Empty;

    /// <summary>Trạng thái vòng đời: UPLOADING, READY, FAILED, DELETING.</summary>
    public string trang_thai { get; set; } = "UPLOADING";

    /// <summary>Chuỗi đại diện Blurhash để client hiển thị placeholder tức thì.</summary>
    public string? blurhash { get; set; }

    /// <summary>Chế độ truy cập: cong_khai hoặc rieng_tu.</summary>
    public string che_do_truy_cap { get; set; } = "cong_khai";

    /// <summary>Thời điểm tạo (UTC).</summary>
    public DateTime ngay_tao { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm cập nhật gần nhất (UTC).</summary>
    public DateTime? ngay_cap_nhat { get; set; }

    /// <summary>Cờ xóa mềm.</summary>
    public bool is_deleted { get; set; }
}
