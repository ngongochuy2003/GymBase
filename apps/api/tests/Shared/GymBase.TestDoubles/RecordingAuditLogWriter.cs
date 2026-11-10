using System.Threading.Channels;
using GymBase.Base.Logging;

namespace GymBase.TestDoubles;

/// <summary>
/// Giữ các bản ghi audit trong bộ nhớ. Audit được ghi sau khi response xong, nên test đợi bằng <see cref="NextAsync"/>.
/// </summary>
public sealed class RecordingAuditLogWriter : IAuditLogWriter
{
    private readonly Channel<AuditLogEntry> _entries = Channel.CreateUnbounded<AuditLogEntry>();

    /// <summary>Đặt exception để giả lập DB lỗi; bản ghi vẫn được đưa vào hàng đợi trước khi ném.</summary>
    public Exception? FailWith { get; set; }

    /// <inheritdoc />
    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        _entries.Writer.TryWrite(entry);
        return FailWith is null ? Task.CompletedTask : Task.FromException(FailWith);
    }

    /// <summary>Đợi bản ghi kế tiếp, hết 5 giây thì ném <see cref="OperationCanceledException"/>.</summary>
    public async Task<AuditLogEntry> NextAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        return await _entries.Reader.ReadAsync(timeout.Token);
    }

    /// <summary>Có bản ghi nào đang chờ đọc không.</summary>
    public bool HasPending => _entries.Reader.TryPeek(out _);
}
