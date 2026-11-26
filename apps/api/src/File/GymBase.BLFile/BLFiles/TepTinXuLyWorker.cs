using GymBase.BLFile.BLStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

/// <summary>Quét và xử lý ảnh chờ xử lý định kỳ trong host API. Còn việc thì làm tiếp ngay, hết việc mới chờ lượt sau.</summary>
public sealed class TepTinXuLyWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<StorageOptions> options,
    ILogger<TepTinXuLyWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.ProcessingIntervalSeconds));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                int daXuLy;
                do
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    daXuLy = await scope.ServiceProvider.GetRequiredService<TepTinXuLyService>().BLXuLyTepChoXuLyAsync(stoppingToken);
                }
                while (daXuLy > 0 && !stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Lượt xử lý ảnh nền lỗi; thử lại ở lượt sau.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
