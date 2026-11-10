using GymBase.Base.Auth;
using GymBase.Base.Crud;
using GymBase.Base.Exceptions;
using FluentValidation;
using GymBase.DLAuth.Contract;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.NguoiDung;

/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho controller web (Controllers/) và mobile (Mob/Controllers/).
/// CRUD lấy từ <see cref="BaseService{TEntity,TDto,TCreateDto,TUpdateDto}"/>; ở đây map entity ↔ DTO
/// và chặn trùng tên đăng nhập.
/// </summary>
public sealed class UserService(
    IUserRepository userRepository,
    TimeProvider timeProvider,
    ICurrentUser currentUser,
    IValidator<NguoiDungCreateDto> createValidator,
    IValidator<NguoiDungUpdateDto> updateValidator,
    IPasswordHasher<nguoi_dung> passwordHasher)
    : BaseService<nguoi_dung, NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(
        userRepository, timeProvider, currentUser, createValidator, updateValidator)
{
    protected override string EntityDisplayName => "người dùng";

    /// <exception cref="ConflictException">
    /// <see cref="AuthErrorCodes.UserUsernameTaken"/>: đã có người dùng còn hiệu lực trùng tai_khoan
    /// (không phân biệt hoa thường). Tên đăng nhập của người dùng đã xóa được dùng lại.
    /// </exception>
    protected override async Task BeforeCreateAsync(NguoiDungCreateDto dto, CancellationToken cancellationToken)
    {
        var taiKhoan = dto.tai_khoan.Trim();
        if (await userRepository.ExistsByTaiKhoanAsync(taiKhoan, cancellationToken))
        {
            throw new ConflictException(
                nameof(NguoiDungCreateDto.tai_khoan),
                $"Tên đăng nhập '{taiKhoan}' đã tồn tại.",
                AuthErrorCodes.UserUsernameTaken);
        }
    }

    protected override NguoiDungDto ToDto(nguoi_dung entity) => new(
        entity.Id,
        entity.tai_khoan,
        entity.ho_ten,
        entity.email,
        entity.so_dien_thoai,
        entity.vai_tro,
        entity.dang_hoat_dong,
        entity.ngay_tao,
        entity.ngay_chinh_sua);

    protected override nguoi_dung CreateEntity(NguoiDungCreateDto dto)
    {
        var user = new nguoi_dung
        {
            tai_khoan = dto.tai_khoan.Trim(),
            ho_ten = dto.ho_ten.Trim(),
            email = dto.email?.Trim(),
            so_dien_thoai = dto.so_dien_thoai?.Trim(),
            vai_tro = dto.vai_tro,
        };
        user.mat_khau_hash = passwordHasher.HashPassword(user, dto.mat_khau);
        return user;
    }

    protected override void ApplyUpdate(nguoi_dung entity, NguoiDungUpdateDto dto)
    {
        entity.ho_ten = dto.ho_ten.Trim();
        entity.email = dto.email?.Trim();
        entity.so_dien_thoai = dto.so_dien_thoai?.Trim();
        entity.vai_tro = dto.vai_tro;
        entity.dang_hoat_dong = dto.dang_hoat_dong;
    }
}
