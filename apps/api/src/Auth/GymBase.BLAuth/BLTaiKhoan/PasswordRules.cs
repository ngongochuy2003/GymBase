using FluentValidation;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Chính sách mật khẩu, xem docs/api/xac-thuc.md mục 5.</summary>
public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    /// <summary>
    /// Quy tắc cho một mật khẩu mới: chỉ ràng buộc độ dài (tối thiểu 8 ký tự; tối đa 128 để chuỗi quá dài không làm tốn tài
    /// nguyên băm). Không có danh sách mật khẩu phổ biến, không cấm mật khẩu chứa email hay tên đăng nhập (chốt 15/11).
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidNewPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(MinLength)
            .MaximumLength(MaxLength)
            .WithName("Mật khẩu");
}
