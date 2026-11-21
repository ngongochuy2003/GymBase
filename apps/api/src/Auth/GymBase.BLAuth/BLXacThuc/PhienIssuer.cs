using System.Security.Cryptography;
using System.Text;
using GymBase.Base.Auth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.Entities.Auth;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Cấp phiên đăng nhập và cặp token cho các service xác thực (đăng nhập, Google, làm mới token). Giữ một chỗ các hằng số
/// thời hạn refresh token và cách băm token.
/// </summary>
public sealed class PhienIssuer(IPhienDangNhapRepository sessions, ITokenIssuer tokenIssuer, TimeProvider timeProvider)
{
    /// <summary>Refresh token hết hạn nếu không dùng trong khoảng này.</summary>
    public static readonly TimeSpan RefreshInactivity = TimeSpan.FromDays(30);

    /// <summary>Hết hạn tuyệt đối của phiên, dù vẫn đang dùng.</summary>
    public static readonly TimeSpan RefreshMaxLifetime = TimeSpan.FromDays(90);

    /// <summary>Tạo phiên mới (chỉ lưu SHA-256 của refresh token) và trả cặp token.</summary>
    public async Task<KetQuaDangNhapDto> StartAsync(tai_khoan user, string? deviceName, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var refreshToken = NewRefreshToken();
        var session = new phien_dang_nhap
        {
            tai_khoan_id = user.Id,
            token_hash = Hash(refreshToken),
            ngay_het_han = now + RefreshInactivity,
            ngay_het_han_toi_da = now + RefreshMaxLifetime,
            ten_thiet_bi = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim(),
            ngay_tao = now,
        };
        await sessions.DLInsertAsync(session, cancellationToken);

        return BuildResult(user, session.Id, refreshToken);
    }

    public KetQuaDangNhapDto BuildResult(tai_khoan user, Guid sessionId, string refreshToken)
    {
        // Không đưa email vào token (docs/api/xac-thuc.md: token không chứa dữ liệu cá nhân); không có tên thì dùng id.
        var name = user.ten_dang_nhap ?? user.Id.ToString();
        var token = tokenIssuer.CreateAccessToken(new AccessTokenRequest(user.Id, name, user.vai_tro, sessionId));
        return new KetQuaDangNhapDto(token.Token, token.ExpiresInSeconds, refreshToken, TaiKhoanMapper.ToDto(user));
    }

    public static string NewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
