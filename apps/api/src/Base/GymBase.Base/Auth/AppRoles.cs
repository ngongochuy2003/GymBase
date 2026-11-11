namespace GymBase.Base.Auth;

/// <summary>
/// Các giá trị hợp lệ của cột vai_tro, cũng là claim role trong token. Đây là vai trò hệ thống.
/// Việc một người có được làm PT hay không dựa vào hồ sơ (loai_nguoi_dung, trang_thai_xac_minh), không dựa vào đây.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal) { Admin, User };
}
