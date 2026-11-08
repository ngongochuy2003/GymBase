using GymBase.Base.Auth;
using GymBase.Base.Crud;
using GymBase.Base.Exceptions;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.Contract.Entities;

namespace GymBase.BLAuth.NguoiDung;

/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho controller web (Controllers/) và mobile (Mob/Controllers/).
/// CRUD lấy từ <see cref="BaseService{TEntity,TDto,TCreateDto,TUpdateDto}"/>; ở đây map entity ↔ DTO
/// và chặn trùng tên đăng nhập.
/// </summary>
public sealed class UserService(IUserRepository userRepository, TimeProvider timeProvider, ICurrentUser currentUser)
    : BaseService<nguoi_dung, NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userRepository, timeProvider, currentUser)
{
    protected override string EntityDisplayName => "người dùng";

    /// <exception cref="ConflictException">
    /// <see cref="AuthErrorCodes.UserUsernameTaken"/>: đã có người dùng còn hiệu lực trùng tai_khoan
    /// (không phân biệt hoa thường). Tên đăng nhập của người dùng đã xóa được dùng lại.
    /// </exception>
    public override async Task<NguoiDungDto> CreateAsync(NguoiDungCreateDto dto, CancellationToken cancellationToken)
    {
        var taiKhoan = dto.tai_khoan.Trim();
        if (await userRepository.ExistsByTaiKhoanAsync(taiKhoan, cancellationToken))
        {
            throw new ConflictException(
                nameof(NguoiDungCreateDto.tai_khoan),
                $"Tên đăng nhập '{taiKhoan}' đã tồn tại.",
                AuthErrorCodes.UserUsernameTaken);
        }

        return await base.CreateAsync(dto, cancellationToken);
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

    protected override nguoi_dung CreateEntity(NguoiDungCreateDto dto) => new()
    {
        tai_khoan = dto.tai_khoan.Trim(),
        ho_ten = dto.ho_ten.Trim(),
        email = dto.email?.Trim(),
        so_dien_thoai = dto.so_dien_thoai?.Trim(),
        vai_tro = dto.vai_tro,
    };

    protected override void ApplyUpdate(nguoi_dung entity, NguoiDungUpdateDto dto)
    {
        entity.ho_ten = dto.ho_ten.Trim();
        entity.email = dto.email?.Trim();
        entity.so_dien_thoai = dto.so_dien_thoai?.Trim();
        entity.vai_tro = dto.vai_tro;
        entity.dang_hoat_dong = dto.dang_hoat_dong;
    }
}
