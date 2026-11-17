using System.Text.RegularExpressions;
using FluentValidation;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Số điện thoại di động Việt Nam khi đăng ký: nhận <c>0…</c>, <c>+84…</c>, <c>84…</c>, lưu dạng <c>0xxxxxxxxx</c>. Không bắt buộc.</summary>
public static partial class PhoneRules
{
    [GeneratedRegex(@"^(?:0|84)([35789][0-9]{8})$")]
    private static partial Regex MobilePattern();

    /// <summary>Bỏ khoảng trắng, dấu chấm, gạch, ngoặc và dấu cộng đầu; null nếu không phải số di động Việt Nam.</summary>
    public static string? NormalizeMobile(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(c => c != ' ' && c != '.' && c != '-' && c != '(' && c != ')').ToArray());
        if (digits.StartsWith('+'))
        {
            digits = digits[1..];
        }

        var match = MobilePattern().Match(digits);
        return match.Success ? "0" + match.Groups[1].Value : null;
    }

    /// <summary>Chỉ kiểm tra khi có nhập: để trống (hoặc <c>null</c>) được, có giá trị thì phải là số di động Việt Nam.</summary>
    public static IRuleBuilderOptions<T, string?> ValidOptionalMobile<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(phone => string.IsNullOrWhiteSpace(phone) || NormalizeMobile(phone) is not null)
            .WithMessage("Số điện thoại phải là số di động Việt Nam.")
            .WithName("Số điện thoại");
}
