using GymBase.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymBase.Persistence.Logging;

/// <summary>Mỗi ngày một lần xóa các dòng <c>system_log</c> quá hạn lưu, lần đầu ngay khi app khởi động.</summary>
public sealed class SystemLogCleanupService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    SystemLogRetention retention,
    ILogger<SystemLogCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1), timeProvider);
        try
        {
            do
            {
                await PurgeAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // App đang dừng.
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddDays(-retention.Days);

            var deleted = await db.Set<system_log>().Where(e => e.thoi_diem < cutoff).ExecuteDeleteAsync(cancellationToken);
            logger.LogInformation("Đã xóa {Count} dòng system_log trước {Cutoff:O}.", deleted, cutoff);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Không xóa được system_log quá hạn, sẽ thử lại ở lần chạy sau.");
        }
    }
}

/// <summary>Số ngày giữ audit log.</summary>
/// <param name="Days">Số ngày, lớn hơn 0.</param>
public sealed record SystemLogRetention(int Days);
