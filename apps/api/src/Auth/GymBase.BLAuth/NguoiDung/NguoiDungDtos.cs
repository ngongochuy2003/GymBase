using GymBase.Base.Entities;

namespace GymBase.BLAuth.NguoiDung;

/// <summary>Thông tin người dùng trả ra API. Không chứa mật khẩu hay cờ xóa.</summary>
public sealed record NguoiDungDto(
    Guid Id,
    string tai_khoan,
    string ho_ten,
    string? email,
    string? so_dien_thoai,
    string vai_tro,
    bool dang_hoat_dong,
    DateTime ngay_tao,
    DateTime? ngay_chinh_sua) : IHasId;

/// <summary>Body khi thêm người dùng. Id, ngày tạo không nhận từ client. Quy tắc: <see cref="NguoiDungCreateValidator"/>.</summary>
public sealed class NguoiDungCreateDto
{
    public string tai_khoan { get; init; } = string.Empty;

    /// <summary>Mật khẩu ban đầu do Admin đặt; chỉ lưu bản đã băm.</summary>
    public string mat_khau { get; init; } = string.Empty;

    public string ho_ten { get; init; } = string.Empty;

    public string? email { get; init; }

    public string? so_dien_thoai { get; init; }

    public string vai_tro { get; init; } = string.Empty;
}

/// <summary>Body khi sửa người dùng. Không đổi được tên đăng nhập. Quy tắc: <see cref="NguoiDungUpdateValidator"/>.</summary>
public sealed class NguoiDungUpdateDto
{
    public string ho_ten { get; init; } = string.Empty;

    public string? email { get; init; }

    public string? so_dien_thoai { get; init; }

    public string vai_tro { get; init; } = string.Empty;

    public bool dang_hoat_dong { get; init; } = true;
}
