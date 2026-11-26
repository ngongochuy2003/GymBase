using GymBase.BLThongBao.BLDayThongBao;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.ThongBao;

/// <summary>Đồng hồ chỉnh tay được, để test backoff và hạn giành lại dòng.</summary>
internal sealed class MutableTimeProvider(DateTime utcNow) : TimeProvider
{
    public DateTime Now { get; set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Now, DateTimeKind.Utc));
}

/// <summary>Bản giả của FCM: ghi lại tin đã gửi, kết quả chọn theo token.</summary>
internal sealed class FakePushSender : IPushSender
{
    public List<TinPush> DaGui { get; } = [];

    public Func<TinPush, KetQuaPush> KetQua { get; set; } = _ => new KetQuaPush(KetQuaDay.ThanhCong, "msg-1");

    public Task<KetQuaPush> GuiAsync(TinPush tin, CancellationToken ct = default)
    {
        DaGui.Add(tin);
        return Task.FromResult(KetQua(tin));
    }
}

internal sealed class ThongBaoKit
{
    public static readonly DateTime Now = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    public MutableTimeProvider Time { get; } = new(Now);
    public InMemoryThongBaoRepository ThongBaos { get; } = new(new InMemoryStore<thong_bao>());
    public InMemoryThietBiRepository ThietBis { get; } = new(new InMemoryStore<thiet_bi>());
    public InMemoryCaiDatThongBaoRepository CaiDats { get; } = new(new InMemoryStore<cai_dat_thong_bao>());
    public FakePushSender Sender { get; } = new();
    public ThongBaoOptions Options { get; } = new();

    public async Task<thiet_bi> ThemThietBiAsync(Guid taiKhoanId, string? token, bool hoatDong = true, string ma = "")
    {
        var tb = new thiet_bi
        {
            tai_khoan_id = taiKhoanId,
            ma_thiet_bi = ma.Length > 0 ? ma : Guid.NewGuid().ToString("N"),
            nen_tang = NenTang.android,
            push_token = token,
            dang_hoat_dong = hoatDong,
            ngay_hoat_dong_cuoi = Now,
            ngay_tao = Now,
        };
        await ThietBis.DLInsertAsync(tb, default);
        return tb;
    }

    public async Task<thong_bao> ThemThongBaoAsync(
        Guid taiKhoanId,
        LoaiThongBao loai = LoaiThongBao.tin_nhan,
        KenhThongBao kenh = KenhThongBao.push,
        TrangThaiGui trangThai = TrangThaiGui.cho_gui,
        DateTime? duKien = null,
        DateTime? nhan = null)
    {
        var tb = new thong_bao
        {
            tai_khoan_id = taiKhoanId,
            loai_thong_bao = loai,
            tieu_de = "Bạn có tin nhắn mới",
            noi_dung = "nội dung chi tiết không được đi qua push",
            loai_doi_tuong = "tin_nhan",
            doi_tuong_id = Guid.NewGuid(),
            thoi_diem_du_kien = duKien ?? Now.AddMinutes(-1),
            trang_thai_gui = trangThai,
            kenh = kenh,
            thoi_diem_nhan = nhan,
            ma_chong_trung = Guid.NewGuid().ToString("N"),
            ngay_tao = Now,
        };
        await ThongBaos.DLInsertAsync(tb, default);
        return tb;
    }

    public Task<thong_bao> LayAsync(thong_bao tb) => ThongBaos.DLGetByIdAsync(tb.Id, default)!;

    public ThongBaoDayService DayService() => new(
        ThongBaos, ThietBis, Sender, Microsoft.Extensions.Options.Options.Create(Options), Time,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<ThongBaoDayService>.Instance);
}
