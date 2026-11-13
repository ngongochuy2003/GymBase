namespace GymBase.Base.Auth;

/// <summary>
/// Tên policy dùng trong <c>[Authorize(Policy = ...)]</c>. Policy chỉ trả lời "được dùng chức năng này không"; quyền trên
/// từng bản ghi (đúng học viên, đúng kết nối) vẫn phải kiểm tra trong BL. Chi tiết: docs/api/phan-quyen.md.
/// </summary>
public static class AppPolicies
{
    /// <summary>Vai trò hệ thống là Admin.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Đã chọn nhóm (có hồ sơ).</summary>
    public const string RequireProfile = "RequireProfile";

    /// <summary>Có hồ sơ PT (pt hoặc ca_hai), kể cả chưa xác minh.</summary>
    public const string RequirePtProfile = "RequirePtProfile";

    /// <summary>PT đã xác minh và tài khoản không bị khóa.</summary>
    public const string RequireVerifiedPt = "RequireVerifiedPt";
}
