using GymBase.BLFile.BLStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

/// <summary>Chạy <see cref="TepTinDonDepService"/> định kỳ trong host API. Lỗi một lượt chỉ ghi log, không làm sập host.</summary>
public sealed class TepTinDonDepWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<StorageOptions> options,
    ILogger<TepTinDonDepWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.CleanupIntervalMinutes));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<TepTinDonDepService>().BLDonTepMoCoiAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Lượt dọn tệp mồ côi lỗi; thử lại ở lượt sau.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
