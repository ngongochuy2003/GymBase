using GymBase.DLSystem.Contract.Entities;

namespace GymBase.BLSystem.Users;

/// <summary>Thông tin người dùng trả ra API. Không chứa mật khẩu hay cờ xóa.</summary>
public sealed record UserDto(
    Guid Id,
    string tai_khoan,
    string ho_ten,
    string? email,
    string? so_dien_thoai,
    string vai_tro,
    bool dang_hoat_dong,
    DateTime ngay_tao)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.tai_khoan,
        user.ho_ten,
        user.email,
        user.so_dien_thoai,
        user.vai_tro,
        user.dang_hoat_dong,
        user.ngay_tao);
}
