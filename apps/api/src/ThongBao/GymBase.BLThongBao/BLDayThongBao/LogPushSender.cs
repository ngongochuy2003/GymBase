using Microsoft.Extensions.Logging;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>
/// Không gửi gì, chỉ ghi log rồi báo thành công. Dùng khi chưa có Firebase (<c>ThongBao:PushProvider = LOG</c>) để chạy được
/// luồng hộp thư và worker.
/// </summary>
public sealed class LogPushSender(ILogger<LogPushSender> logger) : IPushSender
{
    public Task<KetQuaPush> GuiAsync(TinPush tin, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Giả lập push: thong_bao={ThongBaoId} loai={Loai} uu_tien={UuTien} token={Token}",
            tin.ThongBaoId, tin.LoaiThongBao, tin.UuTien, tin.PushToken.Length > 8 ? tin.PushToken[..8] + "…" : tin.PushToken);
        return Task.FromResult(new KetQuaPush(KetQuaDay.ThanhCong, $"log-{Guid.NewGuid():N}"));
    }
}
