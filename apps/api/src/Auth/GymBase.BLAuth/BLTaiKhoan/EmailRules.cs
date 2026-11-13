using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Email là định danh đăng nhập (ADR-0001): lưu và so khớp ở dạng chữ thường, bỏ khoảng trắng hai đầu.</summary>
public static class EmailRules
{
    public const int MaxLength = 100;

    private static readonly EmailAddressAttribute Format = new();

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public static bool IsValid(string email) => Format.IsValid(email.Trim());

    /// <summary>Bắt buộc, đúng định dạng (sau khi bỏ khoảng trắng hai đầu), tối đa <see cref="MaxLength"/> ký tự.</summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(IsValid).WithMessage("Email không hợp lệ.")
            .Must(email => email.Trim().Length <= MaxLength).WithMessage($"Email tối đa {MaxLength} ký tự.")
            .WithName("Email");
}
