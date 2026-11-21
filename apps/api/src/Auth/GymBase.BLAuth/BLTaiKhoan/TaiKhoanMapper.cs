using GymBase.Entities.Auth;

namespace GymBase.BLAuth.BLTaiKhoan;

/// <summary>Chuyển <see cref="tai_khoan"/> sang <see cref="TaiKhoanDto"/>; một nơi duy nhất để các service dùng chung.</summary>
public static class TaiKhoanMapper
{
    public static TaiKhoanDto ToDto(tai_khoan user) => new(
        user.Id,
        user.ten_dang_nhap,
        user.ho_ten,
        user.email,
        user.so_dien_thoai,
        user.vai_tro,
        user.dang_hoat_dong,
        user.ngay_tao,
        user.ngay_chinh_sua,
        user.is_dang_nhap_lan_dau);
}
