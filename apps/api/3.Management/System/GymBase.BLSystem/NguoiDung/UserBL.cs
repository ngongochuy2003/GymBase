using GymBase.AMISBase.BL;
using GymBase.DLSystem.Contract;
using GymBase.Model.Entities;

namespace GymBase.BLSystem.NguoiDung;

// TODO(ERROR): override CreateAsync để chặn trùng tai_khoan khi đã có exception nghiệp vụ. Mục 7.
/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho SystemAPI (web) và MobAPI (mobile).
/// CRUD lấy từ <see cref="BaseBL{TEntity,TDto,TCreateDto,TUpdateDto}"/>; ở đây chỉ map entity ↔ DTO.
/// </summary>
public sealed class UserBL(IUserDL userDL, TimeProvider timeProvider)
    : BaseBL<nguoi_dung, NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(userDL, timeProvider)
{
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
