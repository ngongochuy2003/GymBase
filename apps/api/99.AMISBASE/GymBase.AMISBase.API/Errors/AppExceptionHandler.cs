using GymBase.AMISBase.BL.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GymBase.AMISBase.API.Errors;

/// <summary>
/// Đổi mọi exception chưa bắt thành ProblemDetails. <see cref="BusinessException"/> giữ mã HTTP,
/// code, detail và errors của nó; exception khác thành 500 với câu chung chung, chi tiết chỉ ghi log.
/// Các trường chung (title, code, trace_id, instance) do <see cref="ProblemDetailsCustomizer"/> điền.
/// </summary>
public sealed class AppExceptionHandler(
    IProblemDetailsService problemDetailsService, ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Client đã ngắt kết nối: không còn ai nhận response, cũng không phải lỗi hệ thống.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request {Method} {Path} bị client hủy.", httpContext.Request.Method, httpContext.Request.Path);
            return true;
        }

        var problem = exception switch
        {
            BusinessException business => FromBusiness(business),
            BadHttpRequestException badRequest => new ProblemDetails { Status = badRequest.StatusCode },
            _ => null,
        };

        if (problem is null)
        {
            logger.LogError(exception, "Lỗi không lường trước khi xử lý {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = ProblemDetailsCustomizer.InternalErrorDetail,
            };
            problem.Extensions[ProblemDetailsCustomizer.CodeKey] = ErrorCodes.InternalError;
        }
        else
        {
            logger.LogInformation("Lỗi nghiệp vụ {Status} {Code} khi xử lý {Method} {Path}: {Detail}",
                problem.Status, problem.Extensions[ProblemDetailsCustomizer.CodeKey],
                httpContext.Request.Method, httpContext.Request.Path, problem.Detail);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails FromBusiness(BusinessException exception)
    {
        ProblemDetails problem = exception.Errors is { } errors
            ? new HttpValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value))
            : new ProblemDetails();

        problem.Status = ToStatusCode(exception.Kind);
        problem.Detail = exception.Message;
        problem.Extensions[ProblemDetailsCustomizer.CodeKey] = exception.Code;
        return problem;
    }

    /// <summary>Loại lỗi nghiệp vụ → mã HTTP. Bảng: apps/api/1.Doc/quy-uoc-response.md, mục 4.1.</summary>
    public static int ToStatusCode(BusinessErrorKind kind) => kind switch
    {
        BusinessErrorKind.Validation => StatusCodes.Status400BadRequest,
        BusinessErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
        BusinessErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        BusinessErrorKind.NotFound => StatusCodes.Status404NotFound,
        BusinessErrorKind.Conflict => StatusCodes.Status409Conflict,
        BusinessErrorKind.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
