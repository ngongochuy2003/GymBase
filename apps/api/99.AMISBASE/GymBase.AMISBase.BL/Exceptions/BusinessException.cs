namespace GymBase.AMISBase.BL.Exceptions;

/// <summary>Loại lỗi nghiệp vụ. Tầng API đổi sang mã HTTP; BL không biết gì về HTTP.</summary>
public enum BusinessErrorKind
{
    /// <summary>HTTP 400.</summary>
    Validation,

    /// <summary>HTTP 401.</summary>
    Unauthorized,

    /// <summary>HTTP 403.</summary>
    Forbidden,

    /// <summary>HTTP 404.</summary>
    NotFound,

    /// <summary>HTTP 409.</summary>
    Conflict,

    /// <summary>HTTP 422.</summary>
    BusinessRule,
}

/// <summary>
/// Lỗi nghiệp vụ có chủ đích, ném từ BL. Global exception handler ở AMISBase.API đổi thành
/// ProblemDetails; controller không try/catch. <see cref="Exception.Message"/> là câu tiếng Việt
/// hiển thị được ngay cho người dùng, nên không đưa chi tiết nội bộ vào đây.
/// </summary>
public abstract class BusinessException : Exception
{
    protected BusinessException(
        BusinessErrorKind kind, string code, string detail, IReadOnlyDictionary<string, string[]>? errors)
        : base(detail)
    {
        Kind = kind;
        Code = code;
        Errors = errors;
    }

    public BusinessErrorKind Kind { get; }

    /// <summary>Mã lỗi trả ở trường <c>code</c>, xem <see cref="ErrorCodes"/>.</summary>
    public string Code { get; }

    /// <summary>Lỗi theo từng trường (khóa là tên trường JSON), trả ở trường <c>errors</c>; null nếu không gắn với trường nào.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    /// <summary>Tạo <see cref="Errors"/> cho một trường.</summary>
    protected static IReadOnlyDictionary<string, string[]> FieldError(string field, string message) =>
        new Dictionary<string, string[]> { [field] = [message] };
}
