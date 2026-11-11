using GymBase.Base.Auth;
using GymBase.Base.Crud;
using GymBase.Base.Exceptions;
using FluentValidation;
using GymBase.DLAuth.Contract;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.TaiKhoan;

/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho controller web (Controllers/) và mobile (Mob/Controllers/).
/// CRUD lấy từ <see cref="BaseService{TEntity,TDto,TCreateDto,TUpdateDto}"/>; ở đây map entity ↔ DTO
/// và chặn trùng tên đăng nhập.
/// </summary>
public sealed class UserService(
    IUserRepository userRepository,
    TimeProvider timeProvider,
    ICurrentUser currentUser,
    IValidator<TaiKhoanCreateDto> createValidator,
    IValidator<TaiKhoanUpdateDto> updateValidator,
    IPasswordHasher<tai_khoan> passwordHasher)
    : BaseService<tai_khoan, TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto>(
        userRepository, timeProvider, currentUser, createValidator, updateValidator)
{
    protected override string EntityDisplayName => "người dùng";

    /// <exception cref="ConflictException">
    /// <see cref="AuthErrorCodes.UserUsernameTaken"/>: đã có người dùng còn hiệu lực trùng ten_dang_nhap
    /// (không phân biệt hoa thường). Tên đăng nhập của người dùng đã xóa được dùng lại.
    /// </exception>
    protected override async Task BeforeCreateAsync(TaiKhoanCreateDto dto, CancellationToken cancellationToken)
    {
        var tenDangNhap = dto.ten_dang_nhap.Trim();
        if (await userRepository.ExistsByTenDangNhapAsync(tenDangNhap, cancellationToken))
        {
            throw new ConflictException(
                nameof(TaiKhoanCreateDto.ten_dang_nhap),
                $"Tên đăng nhập '{tenDangNhap}' đã tồn tại.",
                AuthErrorCodes.UserUsernameTaken);
        }
    }

    protected override TaiKhoanDto ToDto(tai_khoan entity) => new(
        entity.Id,
        entity.ten_dang_nhap,
        entity.ho_ten,
        entity.email,
        entity.so_dien_thoai,
        entity.vai_tro,
        entity.dang_hoat_dong,
        entity.ngay_tao,
        entity.ngay_chinh_sua);

    protected override tai_khoan CreateEntity(TaiKhoanCreateDto dto)
    {
        var user = new tai_khoan
        {
            ten_dang_nhap = dto.ten_dang_nhap.Trim(),
            ho_ten = dto.ho_ten.Trim(),
            email = dto.email?.Trim(),
            so_dien_thoai = dto.so_dien_thoai?.Trim(),
            vai_tro = dto.vai_tro,
        };
        user.mat_khau_hash = passwordHasher.HashPassword(user, dto.mat_khau);
        return user;
    }

    protected override void ApplyUpdate(tai_khoan entity, TaiKhoanUpdateDto dto)
    {
        entity.ho_ten = dto.ho_ten.Trim();
        entity.email = dto.email?.Trim();
        entity.so_dien_thoai = dto.so_dien_thoai?.Trim();
        entity.vai_tro = dto.vai_tro;
        entity.dang_hoat_dong = dto.dang_hoat_dong;
    }
}
