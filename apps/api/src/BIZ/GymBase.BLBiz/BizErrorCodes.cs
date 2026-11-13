namespace GymBase.BLBiz;

/// <summary>
/// Mã lỗi chi tiết của nhóm Biz. Giữ mã HTTP của mã chung tương ứng trong <c>GymBase.Base.Exceptions.ErrorCodes</c>.
/// Thêm mã mới thì thêm cả vào bảng mục 4.2 trong docs/api/quy-uoc-response.md.
/// </summary>
public static class BizErrorCodes
{
    /// <summary>403: PT chưa được xác minh (hoặc tài khoản bị khóa) nên chưa được nhận hay quản lý học viên.</summary>
    public const string PtNotVerified = "PT_NOT_VERIFIED";

    /// <summary>403: không có kết nối PT đang hoạt động với học viên này.</summary>
    public const string ConnectionNotActive = "CONNECTION_NOT_ACTIVE";

    /// <summary>403: kết nối PT không bao gồm lĩnh vực này (<c>tap_luyen</c> hoặc <c>dinh_duong</c>).</summary>
    public const string ScopeNotAllowed = "SCOPE_NOT_ALLOWED";

    /// <summary>403: dữ liệu của học viên chưa được chia sẻ cho người gọi.</summary>
    public const string DataNotShared = "DATA_NOT_SHARED";
}
