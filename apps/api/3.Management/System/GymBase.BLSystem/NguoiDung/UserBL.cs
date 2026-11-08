using GymBase.AMISBase.BL;
using GymBase.AMISBase.BL.Exceptions;
using GymBase.DLSystem.Contract;
using GymBase.Model.Entities;

namespace GymBase.BLSystem.NguoiDung;

/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho SystemAPI (web) và MobAPI (mobile).
/// CRUD lấy từ <see cref="BaseBL{TEntity,TDto,TCreateDto,TUpdateDto}"/>; ở đây map entity ↔ DTO
/// và chặn trùng tên đăng nhập.
/// </summary>
public sealed class UserBL(IUserDL userDL, TimeProvider timeProvider)
    : BaseBL<nguoi_dung, NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userDL, timeProvider)
{
    protected override string EntityDisplayName => "người dùng";

    /// <exception cref="ConflictException">
    /// <see cref="SystemErrorCodes.UserUsernameTaken"/>: đã có người dùng còn hiệu lực trùng tai_khoan
    /// (không phân biệt hoa thường). Tên đăng nhập của người dùng đã xóa được dùng lại.
    /// </exception>
    public override async Task<NguoiDungDto> CreateAsync(NguoiDungCreateDto dto, CancellationToken cancellationToken)
    {
        var taiKhoan = dto.tai_khoan.Trim();
        if (await userDL.ExistsByTaiKhoanAsync(taiKhoan, cancellationToken))
        {
            throw new ConflictException(
                nameof(NguoiDungCreateDto.tai_khoan),
                $"Tên đăng nhập '{taiKhoan}' đã tồn tại.",
                SystemErrorCodes.UserUsernameTaken);
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
