using GymBase.Base.Entities;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Thông tin người dùng trả ra API. Không chứa mật khẩu.</summary>
public sealed record TaiKhoanDto(
    Guid Id,
    string? ten_dang_nhap,
    string ho_ten,
    string? email,
    string? so_dien_thoai,
    string vai_tro,
    bool dang_hoat_dong,
    DateTime ngay_tao,
    DateTime? ngay_chinh_sua) : IHasId;

/// <summary>Body khi thêm người dùng. Id, ngày tạo không nhận từ client. Quy tắc: <see cref="TaiKhoanCreateValidator"/>.</summary>
public sealed class TaiKhoanCreateDto
{
    public string ten_dang_nhap { get; init; } = string.Empty;

    /// <summary>Mật khẩu ban đầu do Admin đặt; chỉ lưu bản đã băm.</summary>
    public string mat_khau { get; init; } = string.Empty;

    public string ho_ten { get; init; } = string.Empty;

    public string? email { get; init; }

    public string? so_dien_thoai { get; init; }

    public string vai_tro { get; init; } = string.Empty;
}

/// <summary>Body khi sửa người dùng. Không đổi được tên đăng nhập. Quy tắc: <see cref="TaiKhoanUpdateValidator"/>.</summary>
public sealed class TaiKhoanUpdateDto
{
    public string ho_ten { get; init; } = string.Empty;

    public string? email { get; init; }

    public string? so_dien_thoai { get; init; }

    public string vai_tro { get; init; } = string.Empty;

    public bool dang_hoat_dong { get; init; } = true;
}
