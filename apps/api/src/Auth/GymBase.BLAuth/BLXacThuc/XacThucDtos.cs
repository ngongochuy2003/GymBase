using GymBase.BLAuth.BLTaiKhoan;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>Body đăng nhập.</summary>
public sealed class DangNhapDto
{
    /// <summary>Email của tài khoản (ADR-0001), không phân biệt hoa thường.</summary>
    public string email { get; init; } = string.Empty;

    public string mat_khau { get; init; } = string.Empty;

    /// <summary>Tên thiết bị để người dùng nhận biết phiên, không bắt buộc.</summary>
    public string? ten_thiet_bi { get; init; }
}

/// <summary>Body người mới tự đăng ký bằng mật khẩu. Quy tắc: <see cref="DangKyValidator"/>.</summary>
public sealed class DangKyDto
{
    public string ho_ten { get; init; } = string.Empty;

    /// <summary>Email đăng nhập, bắt buộc và duy nhất (ADR-0001).</summary>
    public string email { get; init; } = string.Empty;

    /// <summary>Số di động Việt Nam; lưu dạng <c>0xxxxxxxxx</c>. Không xác minh.</summary>
    public string so_dien_thoai { get; init; } = string.Empty;

    public string mat_khau { get; init; } = string.Empty;

    /// <summary>Phải là true: người dùng đã tick đồng ý Điều khoản sử dụng và Chính sách bảo mật.</summary>
    public bool dong_y_dieu_khoan { get; init; }

    // TODO(REFERRAL): mã giới thiệu và điểm thưởng chưa làm; nhận vào cho khớp thiết kế nhưng chưa xử lý.
    public string? ma_gioi_thieu { get; init; }

    /// <summary>Tên thiết bị để người dùng nhận biết phiên, không bắt buộc.</summary>
    public string? ten_thiet_bi { get; init; }
}

/// <summary>Body đăng nhập hoặc đăng ký bằng Google: ID token app lấy từ Google.</summary>
public sealed class DangNhapGoogleDto
{
    public string id_token { get; init; } = string.Empty;

    /// <summary>Tên thiết bị để người dùng nhận biết phiên, không bắt buộc.</summary>
    public string? ten_thiet_bi { get; init; }
}

/// <summary>Body đổi refresh token lấy cặp token mới, hoặc đăng xuất khỏi phiên.</summary>
public sealed class LamMoiTokenDto
{
    public string refresh_token { get; init; } = string.Empty;
}

/// <summary>Body người dùng tự đổi mật khẩu.</summary>
public sealed class DoiMatKhauDto
{
    public string mat_khau_cu { get; init; } = string.Empty;

    public string mat_khau_moi { get; init; } = string.Empty;
}

/// <summary>Body Admin đặt lại mật khẩu cho người dùng.</summary>
public sealed class DatLaiMatKhauDto
{
    public string mat_khau_moi { get; init; } = string.Empty;
}

/// <summary>
/// Kết quả đăng nhập hoặc làm mới token. <c>user.is_dang_nhap_lan_dau</c> cho app biết có hiện onboarding không.
/// </summary>
public sealed record KetQuaDangNhapDto(string access_token, int expires_in, string refresh_token, TaiKhoanDto user);
