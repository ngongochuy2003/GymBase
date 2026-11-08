using System.ComponentModel.DataAnnotations;
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

// TODO(AUTH): nhận mật khẩu, băm rồi lưu vào mat_khau_hash. Mục 12.
/// <summary>Body khi thêm người dùng. Id, ngày tạo, mật khẩu không nhận từ client.</summary>
public sealed class NguoiDungCreateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string tai_khoan { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ho_ten { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? email { get; init; }

    [Phone]
    [StringLength(20)]
    public string? so_dien_thoai { get; init; }

    [Required]
    [StringLength(20)]
    public string vai_tro { get; init; } = string.Empty;
}

/// <summary>Body khi sửa người dùng. Không đổi được tên đăng nhập.</summary>
public sealed class NguoiDungUpdateDto
{
    [Required]
    [StringLength(100)]
    public string ho_ten { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? email { get; init; }

    [Phone]
    [StringLength(20)]
    public string? so_dien_thoai { get; init; }

    [Required]
    [StringLength(20)]
    public string vai_tro { get; init; } = string.Empty;

    public bool dang_hoat_dong { get; init; } = true;
}
