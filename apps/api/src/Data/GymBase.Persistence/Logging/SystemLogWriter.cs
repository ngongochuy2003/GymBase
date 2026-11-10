using GymBase.Base.Logging;
using GymBase.Entities.Logging;
using GymBase.Persistence.Configurations.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.Persistence.Logging;

/// <summary>
/// Lưu audit vào bảng <c>system_log</c> bằng một <see cref="AppDbContext"/> riêng, để không lưu kèm
/// thay đổi còn dang dở trong DbContext của request vừa lỗi.
/// </summary>
public sealed class SystemLogWriter(IServiceScopeFactory scopeFactory) : IAuditLogWriter
{
    /// <inheritdoc />
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Set<system_log>().Add(ToRow(entry));
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Đổi sang dòng trong bảng, cắt bớt giá trị dài hơn cột thay vì để MySQL từ chối.</summary>
    public static system_log ToRow(AuditLogEntry entry) => new()
    {
        nguoi_dung = Truncate(entry.UserName, SystemLogLimits.UserName),
        phuong_thuc = Truncate(entry.Method, SystemLogLimits.Method)!,
        duong_dan = Truncate(entry.Path, SystemLogLimits.Path)!,
        ma_trang_thai = entry.StatusCode,
        thoi_gian_ms = entry.DurationMs,
        trace_id = Truncate(entry.TraceId, SystemLogLimits.TraceId)!,
        ip = Truncate(entry.IpAddress, SystemLogLimits.IpAddress),
        thoi_diem = entry.OccurredAtUtc,
    };

    private static string? Truncate(string? value, int maxLength) =>
        value is { Length: var length } && length > maxLength ? value[..maxLength] : value;
}
