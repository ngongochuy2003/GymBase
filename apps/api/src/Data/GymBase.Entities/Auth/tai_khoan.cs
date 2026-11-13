using GymBase.Base.Entities;

namespace GymBase.Entities.Auth;

/// <summary>
/// Tài khoản hệ thống. Tên class trùng tên bảng <c>tai_khoan</c>, property trùng tên cột.
/// Đăng nhập được bằng tên đăng nhập kèm mật khẩu, hoặc bằng Google (bảng <c>dang_nhap_ngoai</c>).
/// </summary>
public sealed class tai_khoan : BaseAuditableEntity
{
    /// <summary>Tên đăng nhập, duy nhất trong số tài khoản còn hiệu lực. Null với tài khoản chỉ dùng Google.</summary>
    public string? ten_dang_nhap { get; set; }

    public string ho_ten { get; set; } = string.Empty;

    /// <summary>Email để hiển thị; với tài khoản Google thì lấy từ Google. Không dùng làm định danh.</summary>
    public string? email { get; set; }

    public string? so_dien_thoai { get; set; }

    /// <summary>Vai trò hệ thống, một trong <c>AppRoles</c>: Admin hoặc User.</summary>
    public string vai_tro { get; set; } = string.Empty;

    /// <summary>false khi tài khoản bị khóa.</summary>
    public bool dang_hoat_dong { get; set; } = true;

    /// <summary>Mật khẩu đã băm. Null với tài khoản chỉ dùng Google. Không bao giờ trả ra API.</summary>
    public string? mat_khau_hash { get; set; }
}
