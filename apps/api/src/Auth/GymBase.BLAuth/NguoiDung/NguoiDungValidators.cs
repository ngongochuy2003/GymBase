using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Validation;
using GymBase.BLAuth.XacThuc;

namespace GymBase.BLAuth.NguoiDung;

/// <summary>Kiểm tra body khi thêm người dùng. Độ dài khớp cột trong bảng nguoi_dung.</summary>
public sealed class NguoiDungCreateValidator : AppValidator<NguoiDungCreateDto>
{
    public NguoiDungCreateValidator()
    {
        RuleFor(x => x.tai_khoan).NotEmpty().WithName("Tên đăng nhập")
            .Must(value => value.Trim().Length >= 3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự.")
            .MaximumLength(50).WithName("Tên đăng nhập");
        RuleFor(x => x.mat_khau).ValidNewPassword()
            .Must((dto, password) => !PasswordRules.ContainsUserName(password, dto.tai_khoan))
            .WithMessage("Mật khẩu không được chứa tên đăng nhập.");
        RuleFor(x => x.ho_ten).NotEmpty().MaximumLength(100).WithName("Họ tên");
        RuleFor(x => x.email).EmailAddress().MaximumLength(100).WithName("Email");
        RuleFor(x => x.so_dien_thoai).PhoneNumber().WithName("Số điện thoại");
        RuleFor(x => x.vai_tro).NotEmpty().Must(AppRoles.All.Contains).WithMessage(InvalidRole).WithName("Vai trò");
    }

    internal const string InvalidRole = "Vai trò không hợp lệ.";
}

/// <summary>Kiểm tra body khi sửa người dùng.</summary>
public sealed class NguoiDungUpdateValidator : AppValidator<NguoiDungUpdateDto>
{
    public NguoiDungUpdateValidator()
    {
        RuleFor(x => x.ho_ten).NotEmpty().MaximumLength(100).WithName("Họ tên");
        RuleFor(x => x.email).EmailAddress().MaximumLength(100).WithName("Email");
        RuleFor(x => x.so_dien_thoai).PhoneNumber().WithName("Số điện thoại");
        RuleFor(x => x.vai_tro).NotEmpty().Must(AppRoles.All.Contains)
            .WithMessage(NguoiDungCreateValidator.InvalidRole).WithName("Vai trò");
    }
}
