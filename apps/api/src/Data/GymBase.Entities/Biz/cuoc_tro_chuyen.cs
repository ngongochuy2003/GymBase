using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Hội thoại. V1 chỉ có chat 1-1, giữ chỗ cho chat nhóm. Cột ngay_cap_nhat là thời điểm hoạt động gần nhất,
/// không phải lần sửa bản ghi.
/// </summary>
public sealed class cuoc_tro_chuyen : BaseEntity
{
    public LoaiCuocTroChuyen loai_cuoc_tro_chuyen { get; set; }

    /// <summary>Hai Id tài khoản đã sắp xếp, nối bằng dấu |; unique để không tạo trùng chat 1-1.</summary>
    public string? ma_hoi_thoai_doi { get; set; }

    /// <summary>Dành cho chat nhóm, chưa dùng ở V1.</summary>
    public string? ten_nhom { get; set; }

    public DateTime ngay_tao { get; set; }

    /// <summary>Lúc có hoạt động gần nhất (UTC).</summary>
    public DateTime ngay_cap_nhat { get; set; }
}
