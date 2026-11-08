using System.Diagnostics;
using GymBase.Base.Exceptions;
using Microsoft.AspNetCore.Http;

namespace GymBase.Base.Hosting.Errors;

/// <summary>
/// Điền các trường chung cho mọi ProblemDetails, kể cả lỗi do framework tự trả (400 khi body sai,
/// 404 khi sai đường dẫn, 405…): <c>title</c> và <c>detail</c> tiếng Việt, <c>code</c>,
/// <c>trace_id</c>, <c>instance</c>. Quy ước: docs/api/quy-uoc-response.md, mục 3.
/// </summary>
public static class ProblemDetailsCustomizer
{
    public const string CodeKey = "code";
    public const string TraceIdKey = "trace_id";

    /// <summary>Câu trả cho mọi lỗi 500; chi tiết thật chỉ nằm trong log.</summary>
    public const string InternalErrorDetail = "Hệ thống đang gặp sự cố. Vui lòng thử lại sau.";

    private sealed record Defaults(string Code, string Title, string Detail);

    private static readonly Dictionary<int, Defaults> ByStatus = new()
    {
        [400] = new(ErrorCodes.ValidationFailed, "Dữ liệu không hợp lệ", ValidationFailedException.DefaultDetail),
        [401] = new(ErrorCodes.Unauthorized, "Chưa xác thực", UnauthorizedException.DefaultDetail),
        [403] = new(ErrorCodes.Forbidden, "Không có quyền", ForbiddenException.DefaultDetail),
        [404] = new(ErrorCodes.NotFound, "Không tìm thấy", NotFoundException.DefaultDetail),
        [409] = new(ErrorCodes.Duplicate, "Dữ liệu bị xung đột", ConflictException.DefaultDetail),
        [422] = new(ErrorCodes.BusinessRule, "Không thực hiện được", "Thao tác vi phạm quy tắc nghiệp vụ."),
        [500] = new(ErrorCodes.InternalError, "Lỗi hệ thống", InternalErrorDetail),
    };

    /// <summary>Gắn vào <c>ProblemDetailsOptions.CustomizeProblemDetails</c>.</summary>
    public static void Customize(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var status = problem.Status ?? context.HttpContext.Response.StatusCode;
        problem.Status = status;

        var defaults = ByStatus.GetValueOrDefault(status)
            ?? (status >= 500 ? ByStatus[500] with { Code = $"HTTP_{status}" } : null);

        // Lỗi 500 không bao giờ lộ chi tiết, kể cả khi nơi ném đã gán detail.
        if (status >= 500)
        {
            problem.Detail = defaults!.Detail;
        }

        if (defaults is not null)
        {
            problem.Title = defaults.Title;
            problem.Detail ??= defaults.Detail;
        }

        // Mã 4xx ngoài bảng (405, 415…): giữ title của framework, detail vẫn phải hiển thị được.
        problem.Detail ??= "Yêu cầu không hợp lệ.";

        problem.Extensions.TryAdd(CodeKey, defaults?.Code ?? $"HTTP_{status}");
        problem.Instance ??= context.HttpContext.Request.Path;

        // ASP.NET tự thêm "traceId"; đổi sang snake_case cho khớp quy ước tên trường JSON.
        problem.Extensions.Remove("traceId");
        problem.Extensions[TraceIdKey] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    }
}
