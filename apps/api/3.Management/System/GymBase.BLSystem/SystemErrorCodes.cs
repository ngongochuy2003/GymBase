namespace GymBase.BLSystem;

/// <summary>
/// Mã lỗi chi tiết của module System. Giữ mã HTTP của mã chung tương ứng trong
/// <c>GymBase.AMISBase.BL.Exceptions.ErrorCodes</c>. Thêm mã mới thì thêm cả vào bảng mục 4.2
/// trong apps/api/1.Doc/quy-uoc-response.md.
/// </summary>
public static class SystemErrorCodes
{
    /// <summary>409: thêm người dùng trùng tai_khoan với một người dùng còn hiệu lực.</summary>
    public const string UserUsernameTaken = "USER_USERNAME_TAKEN";
}
