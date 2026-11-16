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

    /// <summary>
    /// Tên hiển thị công khai. Bỏ trống thì tạo mới lấy <c>tai_khoan.ho_ten</c>, cập nhật giữ nguyên; có gửi thì ghi.
    /// </summary>
    public string? ten_hien_thi { get; init; }

    /// <summary>Ngày sinh (<c>yyyy-MM-dd</c>), bắt buộc, tuổi 13–100. Riêng tư, không hiện ở hồ sơ công khai.</summary>
    public DateOnly? ngay_sinh { get; init; }

    /// <summary><c>nam</c>, <c>nu</c>, <c>khac</c> hoặc <c>khong_muon_noi</c> (cố định, không có trong danh mục). Riêng tư như <see cref="ngay_sinh"/>.</summary>
    [Required]
    public string gioi_tinh { get; init; } = string.Empty;

    /// <summary>Mã trong danh mục <c>trinh_do_tap</c>.</summary>
    // TODO(PROFILE-PT): đang bắt buộc cho mọi vai trò vì mobile gửi khối học viên cho cả ba; nới ra khi dựng khối PT.
    [Required]
    public string trinh_do_tap { get; init; } = string.Empty;

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

/// <summary>
/// Hồ sơ của <b>chính chủ</b> trả về sau khi xem hoặc lưu. Chưa có chỉ số cơ thể thì <see cref="chi_so_co_the"/> null.
/// <see cref="ngay_sinh"/> và <see cref="gioi_tinh"/> là riêng tư: không đưa vào DTO nào hiển thị người khác.
/// </summary>
public sealed record HoSoDto(
    string loai_nguoi_dung,
    string ten_hien_thi,
    DateOnly? ngay_sinh,
    string? gioi_tinh,
    string? trinh_do_tap,
    string? muc_tieu_tap_luyen,
    string? khu_vuc_hoat_dong,
    IReadOnlyList<string>? chuyen_mon,
    short? so_nam_kinh_nghiem,
    string? gioi_thieu,
    string trang_thai_xac_minh,
    ChiSoCoTheDto? chi_so_co_the);
