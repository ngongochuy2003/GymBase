using System.ComponentModel.DataAnnotations;

namespace GymBase.BLBiz.BLHoSo;

/// <summary>
/// Body của <c>PUT /ho-so</c>: một request chứa cả khối học viên và khối PT, ghi trong một transaction.
/// Đợt này app chỉ gửi khối học viên.
/// </summary>
public sealed class HoSoLuuDto
{
    /// <summary><c>hoc_vien</c>, <c>pt</c> hoặc <c>ca_hai</c>.</summary>
    [Required]
    public string loai_nguoi_dung { get; init; } = string.Empty;

    [Required]
    public string muc_tieu_tap_luyen { get; init; } = string.Empty;

    /// <summary>Chiều cao (cm), 100–250.</summary>
    public decimal? chieu_cao_cm { get; init; }

    /// <summary>Cân nặng (kg), 20–300.</summary>
    public decimal? can_nang_kg { get; init; }

    [Required]
    public string khu_vuc_hoat_dong { get; init; } = string.Empty;

    /// <summary>Mã chuyên môn PT; chỉ nhận khi <see cref="loai_nguoi_dung"/> là <c>pt</c> hoặc <c>ca_hai</c>.</summary>
    public List<string>? chuyen_mon { get; init; }

    /// <summary>Số năm kinh nghiệm, 0–60; chỉ nhận khi là PT.</summary>
    public short? so_nam_kinh_nghiem { get; init; }

    /// <summary>Giới thiệu bản thân; chỉ nhận khi là PT.</summary>
    public string? gioi_thieu { get; init; }
}

/// <summary>Chỉ số cơ thể mới nhất.</summary>
public sealed record ChiSoCoTheDto(DateTime ngay_do, decimal? chieu_cao_cm, decimal? can_nang_kg);

/// <summary>Hồ sơ trả về sau khi xem hoặc lưu. Chưa có chỉ số cơ thể thì <see cref="chi_so_co_the"/> null.</summary>
public sealed record HoSoDto(
    string loai_nguoi_dung,
    string ten_hien_thi,
    string? muc_tieu_tap_luyen,
    string? khu_vuc_hoat_dong,
    IReadOnlyList<string>? chuyen_mon,
    short? so_nam_kinh_nghiem,
    string? gioi_thieu,
    string trang_thai_xac_minh,
    ChiSoCoTheDto? chi_so_co_the);
