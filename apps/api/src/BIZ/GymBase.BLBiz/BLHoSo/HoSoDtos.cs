using System.ComponentModel.DataAnnotations;

namespace GymBase.BLBiz.BLHoSo;

/// <summary>
/// Body của <c>PUT /ho-so</c>: một request chứa cả khối học viên và khối PT, ghi trong một transaction.
/// Khối nào bắt buộc tùy <see cref="loai_nguoi_dung"/>: <c>hoc_vien</c> cần khối học viên, <c>pt</c> cần khối PT,
/// <c>ca_hai</c> cần cả hai. Khối không thuộc vai trò: học viên không được gửi thông tin PT, PT có thể bỏ khối học viên.
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

    /// <summary>Ngày sinh (<c>yyyy-MM-dd</c>), tùy chọn; có gửi thì tuổi 13–100. Riêng tư, không hiện ở hồ sơ công khai.</summary>
    public DateOnly? ngay_sinh { get; init; }

    /// <summary>Tùy chọn: <c>nam</c>, <c>nu</c>, <c>khac</c> hoặc <c>khong_muon_noi</c> (cố định, không có trong danh mục). Riêng tư như <see cref="ngay_sinh"/>.</summary>
    public string? gioi_tinh { get; init; }

    /// <summary>Khối học viên: mã trong danh mục <c>trinh_do_tap</c>. Bắt buộc với <c>hoc_vien</c> và <c>ca_hai</c>.</summary>
    public string? trinh_do_tap { get; init; }

    /// <summary>Khối học viên: mã trong danh mục <c>muc_tieu_tap_luyen</c>. Bắt buộc với <c>hoc_vien</c> và <c>ca_hai</c>.</summary>
    public string? muc_tieu_tap_luyen { get; init; }

    /// <summary>Khối học viên: chiều cao (cm), 100–250. Có một trong hai số đo thì phải có cả hai.</summary>
    public decimal? chieu_cao_cm { get; init; }

    /// <summary>Khối học viên: cân nặng (kg), 20–300.</summary>
    public decimal? can_nang_kg { get; init; }

    /// <summary>Tùy chọn: mã trong danh mục <c>khu_vuc</c>.</summary>
    public string? khu_vuc_hoat_dong { get; init; }

    /// <summary>Khối PT: mã chuyên môn, 1–10 mục. Bắt buộc với <c>pt</c> và <c>ca_hai</c>; học viên không được gửi.</summary>
    public List<string>? chuyen_mon { get; init; }

    /// <summary>Khối PT: số năm kinh nghiệm, 0–60. Bắt buộc với <c>pt</c> và <c>ca_hai</c>.</summary>
    public short? so_nam_kinh_nghiem { get; init; }

    /// <summary>Khối PT: giới thiệu bản thân, tùy chọn, tối đa 1000 ký tự.</summary>
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
