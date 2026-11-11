namespace GymBase.Base.Exceptions;

/// <summary>
/// Mã lỗi chung, trả ra client ở trường <c>code</c> của ProblemDetails. Client rẽ nhánh theo mã này,
/// nên đã đưa ra thì không đổi tên, không đổi nghĩa. Mã chi tiết của từng nghiệp vụ đặt ở BL của
/// nhóm đó (ví dụ <c>AuthErrorCodes</c>). Bảng đầy đủ: docs/api/quy-uoc-response.md, mục 4.
/// </summary>
public static class ErrorCodes
{
    /// <summary>400: nhập sai hoặc thiếu trường; có <c>errors</c> theo từng trường.</summary>
    public const string ValidationFailed = "VALIDATION_FAILED";

    /// <summary>401: chưa đăng nhập, token sai hoặc hết hạn, sai tên đăng nhập hoặc mật khẩu.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>403: đã đăng nhập nhưng không đủ quyền, tài khoản bị khóa.</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>404: không có bản ghi.</summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>409: trùng giá trị phải duy nhất.</summary>
    public const string Duplicate = "DUPLICATE";

    /// <summary>409: không xóa được vì bản ghi đang được bảng khác tham chiếu.</summary>
    public const string InUse = "IN_USE";

    /// <summary>409: bản ghi đã bị người khác sửa trước, client cần tải lại.</summary>
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";

    /// <summary>422: dữ liệu đúng định dạng nhưng vi phạm quy tắc nghiệp vụ.</summary>
    public const string BusinessRule = "BUSINESS_RULE";

    /// <summary>500: lỗi không lường trước.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}
