namespace GymBase.BLAuth;

/// <summary>
/// Mã lỗi chi tiết của nhóm Auth. Giữ mã HTTP của mã chung tương ứng trong
/// <c>GymBase.Base.Exceptions.ErrorCodes</c>. Thêm mã mới thì thêm cả vào bảng mục 4.2
/// trong docs/api/quy-uoc-response.md.
/// </summary>
public static class AuthErrorCodes
{
    /// <summary>409: thêm người dùng trùng tai_khoan với một người dùng còn hiệu lực.</summary>
    public const string UserUsernameTaken = "USER_USERNAME_TAKEN";
}
