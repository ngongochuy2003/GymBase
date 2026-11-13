namespace GymBase.Base.Auth;

/// <summary>Thông tin trong một ID token của Google đã được xác minh.</summary>
/// <param name="Subject">Mã người dùng của Google (<c>sub</c>), định danh duy nhất, không đổi khi email đổi.</param>
/// <param name="Email">Email của tài khoản Google; có thể không có nếu app không xin quyền email.</param>
/// <param name="EmailVerified">Google đã xác minh email lúc phát hành token.</param>
/// <param name="Name">Họ tên hiển thị trên tài khoản Google, có thể không có.</param>
public sealed record GoogleIdentity(string Subject, string? Email, bool EmailVerified, string? Name);

/// <summary>
/// Xác minh ID token do app lấy từ Google: chữ ký bằng khóa công khai của Google, <c>aud</c> là một client ID của
/// dự án, <c>iss</c> là Google, chưa hết hạn. Thiết kế: docs/api/xac-thuc.md mục 8.
/// </summary>
public interface IGoogleIdTokenVerifier
{
    /// <returns>Thông tin trong token, hoặc null khi token không hợp lệ hoặc chưa cấu hình client ID.</returns>
    Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
