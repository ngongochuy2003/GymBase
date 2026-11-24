using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>
/// Phía gửi của outbox (ADR-2, quyết định 3 đến 6): quét dòng <c>thong_bao</c> kênh push đã tới hạn, giành dòng bằng cập nhật có
/// điều kiện, gửi tới mọi thiết bị đang hoạt động của tài khoản, rồi ghi kết quả. Giao ít nhất một lần: lỗi tạm thì thử lại với
/// backoff, token chết thì gỡ token, lỗi cấu hình thì dừng và ghi log mức lỗi.
/// </summary>
public sealed class ThongBaoDayService(
    IThongBaoRepository thongBaos,
    IThietBiRepository thietBis,
    IPushSender sender,
    IOptions<ThongBaoOptions> options,
    TimeProvider timeProvider,
    ILogger<ThongBaoDayService> logger)
{
    private readonly ThongBaoOptions _options = options.Value;

    /// <summary>Xử lý một lượt các dòng đến hạn; trả số dòng đã giành và xử lý (kể cả khi kết quả là lỗi hoặc thử lại).</summary>
    public async Task<int> BLDayThongBaoChoGuiAsync(CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cacDong = await thongBaos.DLFindDueAsync(now, now.AddMinutes(-_options.ReclaimMinutes), _options.BatchSize, ct);

        var thongKe = new ThongKe();
        foreach (var dong in cacDong)
        {
            if (!await thongBaos.DLTryClaimAsync(dong, now, ct))
            {
                continue;
            }

            await DayMotDongAsync(dong.Id, now, thongKe, ct);
            thongKe.Dong++;
        }

        if (thongKe.Dong > 0)
        {
            logger.LogInformation(
                "Đẩy thông báo: dong={Dong} da_gui={DaGui} thu_lai={ThuLai} loi={Loi} huy={Huy} tin_thanh_cong={TinOk} token_chet={TokenChet}",
                thongKe.Dong, thongKe.DaGui, thongKe.ThuLai, thongKe.Loi, thongKe.Huy, thongKe.TinThanhCong, thongKe.TokenChet);
        }

        return thongKe.Dong;
    }

    private async Task DayMotDongAsync(Guid id, DateTime now, ThongKe thongKe, CancellationToken ct)
    {
        // Đọc lại sau khi giành dòng để lấy đúng trạng thái mới nhất.
        var dong = await thongBaos.DLGetByIdAsync(id, ct);
        if (dong == null)
        {
            return;
        }

        var thietBiList = await thietBis.DLFindActiveWithTokenAsync(dong.tai_khoan_id, ct);
        var ketQuas = new List<KetQuaDay>();
        foreach (var thietBi in thietBiList)
        {
            var ket = await GuiMotThietBiAsync(dong, thietBi, ct);
            ketQuas.Add(ket);
            switch (ket)
            {
                case KetQuaDay.ThanhCong:
                    thongKe.TinThanhCong++;
                    break;
                case KetQuaDay.TokenChet:
                    thongKe.TokenChet++;
                    await thietBis.DLClearPushTokenAsync(thietBi.Id, ct);
                    break;
                case KetQuaDay.LoiCauHinh:
                    // Thường là cấu hình Firebase sai, ảnh hưởng mọi thiết bị: ghi mức lỗi để cảnh báo.
                    logger.LogError("Push lỗi cấu hình, thong_bao={ThongBaoId}: kiểm tra Firebase và service account.", dong.Id);
                    break;
            }
        }

        KetThuc(dong, ketQuas, now, thongKe);
        await thongBaos.DLUpdateAsync(dong, ct);
    }

    /// <summary>Lỗi bất ngờ của sender (mạng đứt, timeout) được coi là lỗi tạm để dòng không mất.</summary>
    private async Task<KetQuaDay> GuiMotThietBiAsync(thong_bao dong, thiet_bi thietBi, CancellationToken ct)
    {
        var tin = new TinPush(
            thietBi.push_token!,
            dong.Id,
            dong.loai_thong_bao.ToString(),
            dong.loai_doi_tuong,
            dong.doi_tuong_id,
            dong.tieu_de,
            ThongBaoPolicy.UuTien(dong.loai_thong_bao),
            ThongBaoPolicy.Ttl(dong.loai_thong_bao));

        try
        {
            var ketQua = await sender.GuiAsync(tin, ct);
            if (ketQua.Ket != KetQuaDay.ThanhCong)
            {
                logger.LogWarning(
                    "Push thong_bao={ThongBaoId} thiet_bi={ThietBiId} kết quả {Ket}: {Loi}", dong.Id, thietBi.Id, ketQua.Ket, ketQua.Loi);
            }

            return ketQua.Ket;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Push thong_bao={ThongBaoId} thiet_bi={ThietBiId} ném lỗi, coi là lỗi tạm.", dong.Id, thietBi.Id);
            return KetQuaDay.LoiTam;
        }
    }

    /// <summary>
    /// Có ít nhất một thiết bị nhận thì <c>da_gui</c>. Không thì: lỗi cấu hình là <c>loi</c> ngay; lỗi tạm thì thử lại tới hết số
    /// lần rồi <c>loi</c>; còn lại (không có thiết bị, hoặc mọi token đều chết) là <c>huy</c> vì không còn nơi để push, dòng vẫn
    /// nằm trong hộp thư.
    /// </summary>
    private void KetThuc(thong_bao dong, List<KetQuaDay> ketQuas, DateTime now, ThongKe thongKe)
    {
        dong.thoi_diem_nhan = null;

        if (ketQuas.Contains(KetQuaDay.ThanhCong))
        {
            dong.trang_thai_gui = TrangThaiGui.da_gui;
            dong.thoi_diem_da_gui = now;
            thongKe.DaGui++;
        }
        else if (ketQuas.Contains(KetQuaDay.LoiCauHinh))
        {
            dong.trang_thai_gui = TrangThaiGui.loi;
            thongKe.Loi++;
        }
        else if (ketQuas.Contains(KetQuaDay.LoiTam))
        {
            dong.so_lan_thu++;
            if (dong.so_lan_thu >= _options.MaxAttempts)
            {
                dong.trang_thai_gui = TrangThaiGui.loi;
                thongKe.Loi++;
            }
            else
            {
                dong.trang_thai_gui = TrangThaiGui.cho_gui;
                dong.thoi_diem_thu_lai = now + Backoff(dong.so_lan_thu);
                thongKe.ThuLai++;
            }
        }
        else
        {
            dong.trang_thai_gui = TrangThaiGui.huy;
            thongKe.Huy++;
        }
    }

    /// <summary>Lần lỗi thứ n chờ <c>BackoffSeconds * 2^(n-1)</c>, không quá <c>MaxBackoffSeconds</c>.</summary>
    private TimeSpan Backoff(int soLanDaThu)
    {
        var giay = _options.BackoffSeconds * Math.Pow(2, Math.Min(soLanDaThu - 1, 20));
        return TimeSpan.FromSeconds(Math.Min(giay, _options.MaxBackoffSeconds));
    }

    private sealed class ThongKe
    {
        public int Dong, DaGui, ThuLai, Loi, Huy, TinThanhCong, TokenChet;
    }
}
