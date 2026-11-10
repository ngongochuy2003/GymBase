using System.Globalization;
using FluentValidation;

namespace GymBase.Base.Validation;

/// <summary>
/// Lớp cha của mọi validator: thông báo tiếng Việt, và mỗi trường dừng ở lỗi đầu tiên để người dùng
/// chỉ thấy một lỗi dưới mỗi ô nhập.
/// </summary>
public abstract class AppValidator<T> : AbstractValidator<T>
{
    static AppValidator()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("vi");
    }

    protected AppValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}

/// <summary>Quy tắc dùng chung cho nhiều validator.</summary>
public static class ValidationRules
{
    /// <summary>Số điện thoại: 7 đến 20 ký tự gồm số, khoảng trắng, dấu ngoặc, gạch, chấm, có thể bắt đầu bằng +. Null là hợp lệ.</summary>
    public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Matches(@"^\+?[0-9 ().\-]{7,20}$").WithMessage("'{PropertyName}' không đúng định dạng.");
}
