namespace GymBase.BLAuth;

/// <summary>
/// Mã lỗi chi tiết của nhóm Auth. Giữ mã HTTP của mã chung tương ứng trong
/// <c>GymBase.Base.Exceptions.ErrorCodes</c>. Thêm mã mới thì thêm cả vào bảng mục 4.2
/// trong docs/api/quy-uoc-response.md.
/// </summary>
public static class AuthErrorCodes
{
    /// <summary>409: thêm người dùng trùng ten_dang_nhap với một người dùng còn hiệu lực.</summary>
    public const string UserUsernameTaken = "USER_USERNAME_TAKEN";

    /// <summary>409: thêm hoặc sửa người dùng trùng email với một người dùng khác (không phân biệt hoa thường).</summary>
    public const string UserEmailTaken = "USER_EMAIL_TAKEN";

    /// <summary>403: chức năng cần hồ sơ (nhóm người dùng) mà tài khoản chưa có; app dẫn người dùng tới bước thiết lập hồ sơ.</summary>
    public const string ProfileRequired = "PROFILE_REQUIRED";

    /// <summary>409: đăng nhập Google mà email đã thuộc một tài khoản có mật khẩu (không tự liên kết theo email, ADR-0001).</summary>
    public const string EmailUsedPassword = "AUTH_EMAIL_USED_PASSWORD";

    /// <summary>401: ID token của Google không hợp lệ (sai chữ ký, aud, iss, hết hạn) hoặc chưa cấu hình client ID.</summary>
    public const string GoogleTokenInvalid = "AUTH_GOOGLE_TOKEN_INVALID";

    /// <summary>401: tài khoản Google mới chưa có email đã xác minh nên không thể tạo tài khoản GymBase.</summary>
    public const string GoogleEmailUnverified = "AUTH_GOOGLE_EMAIL_UNVERIFIED";

    /// <summary>401: đăng nhập không thành công, không nói rõ lý do để tránh lộ trạng thái tài khoản.</summary>
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
}
