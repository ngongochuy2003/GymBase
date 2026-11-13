using FluentValidation;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Chính sách mật khẩu, xem docs/api/xac-thuc.md mục 5.</summary>
public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    private const int MinUserNameToCheck = 3;

    private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
    {
        "12345678", "123456789", "1234567890", "12341234", "11111111", "00000000", "88888888", "66668888",
        "87654321", "password", "password1", "password123", "passw0rd", "qwertyui", "qwerty123", "qwertyuiop",
        "1q2w3e4r", "1qaz2wsx", "abc12345", "abcd1234", "a1234567", "a123456789", "aa123456", "admin123",
        "administrator", "iloveyou", "welcome1", "letmein1", "matkhau", "matkhau1", "matkhau123", "anhyeuem",
        "emyeuanh", "gymbase1", "gymbase123",
    };

    /// <summary>Quy tắc cho một mật khẩu mới: độ dài và không nằm trong danh sách mật khẩu phổ biến.</summary>
    public static IRuleBuilderOptions<T, string> ValidNewPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(MinLength)
            .MaximumLength(MaxLength)
            .Must(password => !Common.Contains(password)).WithMessage("Mật khẩu này quá phổ biến, hãy chọn mật khẩu khác.")
            .WithName("Mật khẩu");

    /// <summary>Mật khẩu có chứa tên đăng nhập hay không, không phân biệt hoa thường.</summary>
    public static bool ContainsUserName(string password, string? userName)
    {
        var name = userName?.Trim();
        return name is { Length: >= MinUserNameToCheck } && password.Contains(name, StringComparison.OrdinalIgnoreCase);
    }
}
