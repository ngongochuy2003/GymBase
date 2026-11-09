using System.Diagnostics;
using GymBase.Base.Auth;
using GymBase.Base.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GymBase.Base.Hosting.Logging;

/// <summary>
/// Ghi audit cho các request thay đổi dữ liệu (POST, PUT, PATCH, DELETE) đã vào tới action.
/// Bản ghi được lưu sau khi response hoàn tất để có mã HTTP cuối cùng, kể cả khi action ném exception.
/// Lỗi khi lưu audit chỉ ghi ra log kỹ thuật, không ảnh hưởng request.
/// Request bị chặn ở bước xác thực, phân quyền (401, 403) không đi qua filter này.
/// </summary>
public sealed class AuditLogFilter(
    IAuditLogWriter writer,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<AuditLogFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        if (!IsWrite(http.Request.Method))
        {
            await next();
            return;
        }

        var startedAt = timeProvider.GetTimestamp();
        var clientCancelled = false;
        var entry = new AuditLogEntry(
            currentUser.UserName,
            http.Request.Method,
            http.Request.Path.Value ?? string.Empty,
            StatusCode: 0,
            DurationMs: 0,
            Activity.Current?.TraceId.ToString() ?? http.TraceIdentifier,
            // TODO(HOST): sau Nginx đây là IP của proxy, cần bật ForwardedHeaders. Mục 11, 14.
            http.Connection.RemoteIpAddress?.ToString(),
            timeProvider.GetUtcNow().UtcDateTime);

        http.Response.OnCompleted(() => SaveAsync(entry with
        {
            StatusCode = clientCancelled ? StatusCodes.Status499ClientClosedRequest : http.Response.StatusCode,
            DurationMs = (long)timeProvider.GetElapsedTime(startedAt).TotalMilliseconds,
        }));

        var executed = await next();
        clientCancelled = executed.Exception is OperationCanceledException && http.RequestAborted.IsCancellationRequested;
    }

    private static bool IsWrite(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private async Task SaveAsync(AuditLogEntry entry)
    {
        try
        {
            await writer.WriteAsync(entry, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không lưu được audit log của {Method} {Path}.", entry.Method, entry.Path);
        }
    }
}
