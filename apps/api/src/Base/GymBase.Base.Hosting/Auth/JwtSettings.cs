using System.Text;
using Microsoft.Extensions.Configuration;

namespace GymBase.Base.Hosting.Auth;

/// <summary>Tên claim trong access token.</summary>
public static class JwtClaimNames
{
    public const string Subject = "sub";
    public const string Session = "sid";
    public const string TokenId = "jti";
    public const string UserName = "preferred_username";
    public const string Role = "role";
}

/// <summary>Cấu hình ký và kiểm tra access token, đọc từ mục <c>Jwt</c>.</summary>
public sealed class JwtSettings
{
    public const string TokenType = "at+jwt";
    private const int MinKeyBytes = 32;

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required byte[] SigningKey { get; init; }

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <exception cref="InvalidOperationException">Thiếu khóa ký, khóa không phải Base64 hoặc ngắn hơn 256 bit.</exception>
    public static JwtSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var key = section["SigningKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình Jwt:SigningKey. Khi dev, đặt bằng dotnet user-secrets (xem docs/api/cach-chay-api.md).");
        }

        var bytes = new byte[Encoding.ASCII.GetMaxCharCount(key.Length)];
        if (!Convert.TryFromBase64String(key, bytes, out var length) || length < MinKeyBytes)
        {
            throw new InvalidOperationException("Jwt:SigningKey phải là chuỗi Base64 của ít nhất 32 byte ngẫu nhiên.");
        }

        return new JwtSettings
        {
            Issuer = section["Issuer"] ?? "gymbase-api",
            Audience = section["Audience"] ?? "gymbase-api",
            SigningKey = bytes[..length],
        };
    }
}
