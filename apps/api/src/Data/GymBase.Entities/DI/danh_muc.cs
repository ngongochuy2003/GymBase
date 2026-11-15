using GymBase.Base.Entities;

namespace GymBase.Entities.DI;

/// <summary>
/// Một mục danh mục dùng cho dropdown và kiểm tra mã. Khóa theo cặp (<c>ma_dinh_danh</c>, <c>ma</c>): cùng một mã
/// được dùng ở nhiều loại, ví dụ <c>tang_co</c> có ở cả <c>muc_tieu_tap_luyen</c> và <c>chuyen_mon_pt</c>.
/// </summary>
public sealed class danh_muc : BaseAuditableEntity
{
    /// <summary>Loại danh mục (namespace), ví dụ <c>muc_tieu_tap_luyen</c>, <c>khu_vuc</c>, <c>chuyen_mon_pt</c>.</summary>
    public string ma_dinh_danh { get; set; } = string.Empty;

    /// <summary>Mã mục trong loại, ví dụ <c>tang_co</c>. Hồ sơ lưu mã này chứ không lưu tên hiển thị.</summary>
    public string ma { get; set; } = string.Empty;

    /// <summary>Tên hiển thị; sửa ở đây là mọi màn hình đổi theo.</summary>
    public string ten { get; set; } = string.Empty;

    public string? mo_ta { get; set; }

    /// <summary>
    /// Đường dẫn của mục trong cây, theo tên hiển thị. Đợt này danh mục phẳng nên bằng <see cref="ma"/>.
    /// TODO(DANH-MUC): chốt định dạng đường dẫn trước khi dùng cho danh mục nhiều cấp (khu vực → quận/huyện).
    /// </summary>
    public string? muc_luc { get; set; }

    /// <summary>Như <see cref="muc_luc"/> nhưng theo mã, ví dụ <c>khu_vuc/ha_noi</c>. Có index để lọc cây con.</summary>
    public string? muc_luc_ma { get; set; }

    /// <summary>Độ sâu trong cây; cấp gốc là 1.</summary>
    public int? cap { get; set; }

    /// <summary>Mục cha; null với mục gốc.</summary>
    public Guid? danh_muc_cap_tren_id { get; set; }

    /// <summary>True: đang dùng (hiện trong dropdown); false: ẩn nhưng giữ dữ liệu cũ.</summary>
    public bool tinh_trang { get; set; } = true;

    public string? ghi_chu { get; set; }
}
