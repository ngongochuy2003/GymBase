using GymBase.Base.Entities;

namespace GymBase.Entities.Auth;

/// <summary>
/// Tài khoản hệ thống. Tên class trùng tên bảng <c>tai_khoan</c>, property trùng tên cột.
/// Đăng nhập được bằng email kèm mật khẩu, hoặc bằng Google (bảng <c>dang_nhap_ngoai</c>). Email là định danh (ADR-0001).
/// </summary>
public sealed class tai_khoan : BaseAuditableEntity
{
    /// <summary>Tên đăng nhập (không bắt buộc), duy nhất. Null với tài khoản tự đăng ký hoặc chỉ dùng Google.</summary>
    public string? ten_dang_nhap { get; set; }

    public string ho_ten { get; set; } = string.Empty;

    /// <summary>Email đăng nhập, định danh của tài khoản (ADR-0001); với tài khoản Google thì lấy từ Google.</summary>
    public string? email { get; set; }

    public string? so_dien_thoai { get; set; }

    /// <summary>Vai trò hệ thống, một trong <c>AppRoles</c>: Admin hoặc User.</summary>
    public string vai_tro { get; set; } = string.Empty;

    /// <summary>false khi tài khoản bị khóa.</summary>
    public bool dang_hoat_dong { get; set; } = true;

    /// <summary>Mật khẩu đã băm. Null với tài khoản chỉ dùng Google. Không bao giờ trả ra API.</summary>
    public string? mat_khau_hash { get; set; }

    /// <summary>
    /// true cho tới khi người dùng bấm một trong hai nút của màn onboarding sau lần đăng nhập đầu tiên; app dựa vào đây
    /// để hiện onboarding. Tài khoản tạo mới mặc định true.
    /// </summary>
    public bool is_dang_nhap_lan_dau { get; set; } = true;
}
