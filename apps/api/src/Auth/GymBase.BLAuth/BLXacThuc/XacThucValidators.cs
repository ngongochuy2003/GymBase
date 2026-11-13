using GymBase.BLAuth.BLTaiKhoan;
using FluentValidation;
using GymBase.Base.Validation;

namespace GymBase.BLAuth.BLXacThuc;

public sealed class DangNhapValidator : AppValidator<DangNhapDto>
{
    public DangNhapValidator()
    {
        RuleFor(x => x.email).ValidEmail();

        // Giới hạn độ dài để chuỗi quá dài không làm tốn tài nguyên băm.
        RuleFor(x => x.mat_khau).NotEmpty().MaximumLength(PasswordRules.MaxLength).WithName("Mật khẩu");
        RuleFor(x => x.ten_thiet_bi).MaximumLength(100).WithName("Tên thiết bị");
    }
}

public sealed class DangKyValidator : AppValidator<DangKyDto>
{
    public DangKyValidator()
    {
        RuleFor(x => x.ho_ten).NotEmpty().MaximumLength(100).WithName("Họ tên");
        RuleFor(x => x.email).ValidEmail();
        RuleFor(x => x.so_dien_thoai).ValidMobile();
        RuleFor(x => x.mat_khau).ValidNewPassword()
            .Must((dto, password) => !PasswordRules.ContainsUserName(password, EmailRules.LocalPart(dto.email ?? string.Empty)))
            .WithMessage("Mật khẩu không được chứa email.");
        RuleFor(x => x.dong_y_dieu_khoan).Equal(true)
            .WithMessage("Bạn cần đồng ý Điều khoản sử dụng và Chính sách bảo mật.");
        RuleFor(x => x.ten_thiet_bi).MaximumLength(100).WithName("Tên thiết bị");
    }
}

public sealed class DangNhapGoogleValidator : AppValidator<DangNhapGoogleDto>
{
    public DangNhapGoogleValidator()
    {
        // ID token của Google dài khoảng 1 đến 2 KB; giới hạn để chuỗi quá dài không tốn tài nguyên xác minh.
        RuleFor(x => x.id_token).NotEmpty().MaximumLength(8192).WithName("ID token");
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
