using FluentValidation;
using GymBase.Base.Validation;

namespace GymBase.BLAuth.XacThuc;

public sealed class DangNhapValidator : AppValidator<DangNhapDto>
{
    public DangNhapValidator()
    {
        RuleFor(x => x.ten_dang_nhap).NotEmpty().MaximumLength(50).WithName("Tên đăng nhập");

        // Giới hạn độ dài để chuỗi quá dài không làm tốn tài nguyên băm.
        RuleFor(x => x.mat_khau).NotEmpty().MaximumLength(PasswordRules.MaxLength).WithName("Mật khẩu");
        RuleFor(x => x.ten_thiet_bi).MaximumLength(100).WithName("Tên thiết bị");
    }
}

public sealed class LamMoiTokenValidator : AppValidator<LamMoiTokenDto>
{
    public LamMoiTokenValidator()
    {
        RuleFor(x => x.refresh_token).NotEmpty().MaximumLength(200).WithName("Refresh token");
    }
}

public sealed class DoiMatKhauValidator : AppValidator<DoiMatKhauDto>
{
    public DoiMatKhauValidator()
    {
        RuleFor(x => x.mat_khau_cu).NotEmpty().MaximumLength(PasswordRules.MaxLength).WithName("Mật khẩu hiện tại");
        RuleFor(x => x.mat_khau_moi).ValidNewPassword();
    }
}

public sealed class DatLaiMatKhauValidator : AppValidator<DatLaiMatKhauDto>
{
    public DatLaiMatKhauValidator()
    {
        RuleFor(x => x.mat_khau_moi).ValidNewPassword();
    }
}
