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

/// <summary>Kết quả đăng nhập hoặc làm mới token.</summary>
public sealed record KetQuaDangNhapDto(string access_token, int expires_in, string refresh_token, TaiKhoanDto user);
