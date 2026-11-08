using GymBase.Base.Entities;

namespace GymBase.Entities.Auth;

/// <summary>
/// Người dùng hệ thống. Tên class trùng tên bảng <c>nguoi_dung</c>, property trùng tên cột.
/// </summary>
public sealed class nguoi_dung : BaseAuditableEntity, ISoftDelete
{
    /// <summary>Tên đăng nhập, duy nhất.</summary>
    public string tai_khoan { get; set; } = string.Empty;

    public string ho_ten { get; set; } = string.Empty;

    public string? email { get; set; }

    public string? so_dien_thoai { get; set; }

    /// <summary>Vai trò, một trong <c>AppRoles</c>: Admin, Trainer, Trainee.</summary>
    public string vai_tro { get; set; } = string.Empty;

    /// <summary>false khi tài khoản bị khóa.</summary>
    public bool dang_hoat_dong { get; set; } = true;

    /// <summary>Mật khẩu đã băm. Không bao giờ trả ra API.</summary>
    public string mat_khau_hash { get; set; } = string.Empty;

    public bool is_deleted { get; set; }
}
