using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Quét và đẩy thông báo chờ gửi định kỳ trong host API. Còn việc thì làm tiếp ngay, hết việc mới chờ lượt sau.</summary>
public sealed class ThongBaoDayWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ThongBaoOptions> options,
    ILogger<ThongBaoDayWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.IntervalSeconds));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                int daXuLy;
                do
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    daXuLy = await scope.ServiceProvider.GetRequiredService<ThongBaoDayService>().BLDayThongBaoChoGuiAsync(stoppingToken);
                }
                while (daXuLy > 0 && !stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Lượt đẩy thông báo lỗi; thử lại ở lượt sau.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
