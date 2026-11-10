namespace GymBase.Base.Logging;

/// <summary>Một thao tác ghi dữ liệu qua API. Không chứa body của request để tránh lưu mật khẩu, token.</summary>
/// <param name="UserName">Tên đăng nhập; null khi chưa đăng nhập.</param>
/// <param name="Method">Phương thức HTTP.</param>
/// <param name="Path">Đường dẫn thực tế, không gồm query string.</param>
/// <param name="StatusCode">Mã HTTP đã trả về.</param>
/// <param name="DurationMs">Thời gian xử lý, tính bằng mili giây.</param>
/// <param name="TraceId">Mã để tìm các dòng log kỹ thuật của cùng request.</param>
/// <param name="IpAddress">Địa chỉ IP của bên gọi.</param>
/// <param name="OccurredAtUtc">Thời điểm bắt đầu xử lý (UTC).</param>
public sealed record AuditLogEntry(
    string? UserName,
    string Method,
    string Path,
    int StatusCode,
    long DurationMs,
    string TraceId,
    string? IpAddress,
    DateTime OccurredAtUtc);

/// <summary>Nơi lưu audit log. Tách khỏi lớp Hosting để Hosting không phụ thuộc EF Core.</summary>
public interface IAuditLogWriter
{
    /// <summary>Lưu một bản ghi audit.</summary>
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken);
}
