using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Audit log gộp cho nhật ký, kế hoạch, buổi tập, gói PT và báo cáo. Chỉ thêm, không sửa.
/// </summary>
public sealed class lich_su_chinh_sua : BaseEntity
{
    public LoaiDoiTuongLichSu loai_doi_tuong { get; set; }

    /// <summary>Đa hình, không có khóa ngoại; service kiểm tra.</summary>
    public Guid doi_tuong_id { get; set; }

    public HanhDongLichSu hanh_dong { get; set; }

    public Guid nguoi_thuc_hien_id { get; set; }

    /// <summary>JSON các trường nghiệp vụ trước khi đổi. Không lưu token, khóa S3 hay dữ liệu nhạy cảm.</summary>
    public string? du_lieu_truoc { get; set; }

    /// <summary>JSON các trường sau khi đổi.</summary>
    public string? du_lieu_sau { get; set; }

    /// <summary>Lúc server ghi nhận (UTC).</summary>
    public DateTime ngay_thuc_hien { get; set; }
}
