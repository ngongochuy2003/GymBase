namespace GymBase.AMISBase.BL.Exceptions;

/// <summary>400 <see cref="ErrorCodes.ValidationFailed"/>: dữ liệu nhập sai hoặc thiếu.</summary>
public sealed class ValidationFailedException : BusinessException
{
    public const string DefaultDetail = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";

    public ValidationFailedException(
        IReadOnlyDictionary<string, string[]> errors, string detail = DefaultDetail, string code = ErrorCodes.ValidationFailed)
        : base(BusinessErrorKind.Validation, code, detail, errors)
    {
    }

    /// <summary>Lỗi của một trường; <paramref name="message"/> vừa là detail vừa nằm trong errors.</summary>
    public ValidationFailedException(string field, string message, string code = ErrorCodes.ValidationFailed)
        : base(BusinessErrorKind.Validation, code, message, FieldError(field, message))
    {
    }
}

/// <summary>401 <see cref="ErrorCodes.Unauthorized"/>: chưa đăng nhập, token hết hạn, sai thông tin đăng nhập.</summary>
public sealed class UnauthorizedException(
    string detail = UnauthorizedException.DefaultDetail, string code = ErrorCodes.Unauthorized)
    : BusinessException(BusinessErrorKind.Unauthorized, code, detail, null)
{
    public const string DefaultDetail = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.";
}

/// <summary>403 <see cref="ErrorCodes.Forbidden"/>: không đủ quyền, tài khoản bị khóa.</summary>
public sealed class ForbiddenException(
    string detail = ForbiddenException.DefaultDetail, string code = ErrorCodes.Forbidden)
    : BusinessException(BusinessErrorKind.Forbidden, code, detail, null)
{
    public const string DefaultDetail = "Bạn không có quyền thực hiện thao tác này.";
}

/// <summary>404 <see cref="ErrorCodes.NotFound"/>: không có bản ghi hoặc bản ghi đã xóa.</summary>
public sealed class NotFoundException(
    string detail = NotFoundException.DefaultDetail, string code = ErrorCodes.NotFound)
    : BusinessException(BusinessErrorKind.NotFound, code, detail, null)
{
    public const string DefaultDetail = "Không tìm thấy dữ liệu hoặc dữ liệu đã bị xóa.";
}

/// <summary>409 <see cref="ErrorCodes.Duplicate"/>: trùng giá trị phải duy nhất.</summary>
public sealed class ConflictException : BusinessException
{
    public const string DefaultDetail = "Dữ liệu đã tồn tại.";

    public ConflictException(string detail = DefaultDetail, string code = ErrorCodes.Duplicate)
        : base(BusinessErrorKind.Conflict, code, detail, null)
    {
    }

    /// <summary>Trùng ở một trường; client hiện lỗi dưới ô đó.</summary>
    public ConflictException(string field, string message, string code)
        : base(BusinessErrorKind.Conflict, code, message, FieldError(field, message))
    {
    }
}

/// <summary>409 <see cref="ErrorCodes.ConcurrencyConflict"/>: bản ghi đã bị người khác sửa trước.</summary>
public sealed class ConcurrencyException(
    string detail = ConcurrencyException.DefaultDetail, string code = ErrorCodes.ConcurrencyConflict)
    : BusinessException(BusinessErrorKind.Conflict, code, detail, null)
{
    public const string DefaultDetail = "Dữ liệu đã bị người khác thay đổi. Vui lòng tải lại rồi thử lại.";
}

/// <summary>422 <see cref="ErrorCodes.BusinessRule"/>: đúng định dạng nhưng vi phạm quy tắc nghiệp vụ.</summary>
public sealed class BusinessRuleException(string detail, string code = ErrorCodes.BusinessRule)
    : BusinessException(BusinessErrorKind.BusinessRule, code, detail, null);
