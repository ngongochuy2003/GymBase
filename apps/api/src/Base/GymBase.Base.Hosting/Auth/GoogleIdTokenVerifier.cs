using Google.Apis.Auth;
using GymBase.Base.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymBase.Base.Hosting.Auth;

/// <summary>
/// Xác minh ID token bằng thư viện chính thức của Google (<see cref="GoogleJsonWebSignature"/>): chữ ký bằng khóa công khai
/// của Google (thư viện tự tải và lưu đệm), <c>iss</c> là Google, <c>exp</c> chưa qua, và <c>aud</c> là một trong các client ID
/// khai báo ở <c>Google:ClientIds</c>. Thiết kế: docs/api/xac-thuc.md mục 8.
/// </summary>
public sealed class GoogleIdTokenVerifier(IConfiguration configuration, ILogger<GoogleIdTokenVerifier> logger) : IGoogleIdTokenVerifier
{
    /// <summary>Mục cấu hình chứa danh sách client ID (<c>Google:ClientIds:0</c>, <c>Google:ClientIds:1</c>…).</summary>
    public const string ClientIdsSection = "Google:ClientIds";

    public async Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientIds = (configuration.GetSection(ClientIdsSection).Get<string[]>() ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToArray();
        if (clientIds.Length == 0)
        {
            logger.LogError("Chưa cấu hình {Section}: không thể xác minh ID token của Google.", ClientIdsSection);
            return null;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken, new GoogleJsonWebSignature.ValidationSettings { Audience = clientIds });
            return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name);
        }
        catch (Exception exception) when (exception is InvalidJwtException or FormatException or ArgumentException)
        {
            // Token sai chữ ký, sai aud/iss, hết hạn hoặc không phải JWT: không ghi nội dung token vào log.
            // Lỗi mạng khi tải khóa của Google không bắt ở đây, để thành lỗi 500 chứ không báo nhầm là token sai.
            logger.LogWarning("ID token của Google không hợp lệ: {Reason}", exception.Message);
            return null;
        }
    }
}
