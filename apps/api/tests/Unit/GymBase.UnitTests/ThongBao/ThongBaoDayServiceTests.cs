using GymBase.BLThongBao.BLDayThongBao;
using GymBase.Entities.Biz.Enums;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

public sealed class ThongBaoDayServiceTests
{
    private static readonly DateTime Now = ThongBaoKit.Now;
    private readonly ThongBaoKit _kit = new();
    private readonly Guid _tk = Guid.NewGuid();

    [Fact]
    public async Task DueRow_IsSentToEveryActiveDevice_AndMarkedSent()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        await _kit.ThemThietBiAsync(_tk, "token-b");
        await _kit.ThemThietBiAsync(_tk, "token-tat", hoatDong: false);
        await _kit.ThemThietBiAsync(_tk, null);
        var tb = await _kit.ThemThongBaoAsync(_tk);

        var daXuLy = await _kit.DayService().BLDayThongBaoChoGuiAsync();

        Assert.Equal(1, daXuLy);
        Assert.Equal(["token-a", "token-b"], _kit.Sender.DaGui.Select(t => t.PushToken).Order());
        var sau = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.da_gui, sau.trang_thai_gui);
        Assert.Equal(Now, sau.thoi_diem_da_gui);
    }

    [Fact]
    public async Task Payload_IsNeutral_AndCarriesOnlyIdsAndTitle()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var tb = await _kit.ThemThongBaoAsync(_tk);

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var tin = Assert.Single(_kit.Sender.DaGui);
        Assert.Equal(tb.Id, tin.ThongBaoId);
        Assert.Equal("tin_nhan", tin.LoaiThongBao);
        Assert.Equal("tin_nhan", tin.LoaiDoiTuong);
        Assert.Equal(tb.doi_tuong_id, tin.DoiTuongId);
        Assert.Equal(tb.tieu_de, tin.TieuDe);
        Assert.Null(typeof(TinPush).GetProperty("NoiDung"));
    }

    [Theory]
    [InlineData(LoaiThongBao.nhac_tap, UuTienPush.Cao, 10)]
    [InlineData(LoaiThongBao.tin_nhan, UuTienPush.Cao, 60)]
    [InlineData(LoaiThongBao.danh_gia, UuTienPush.Thuong, 1440)]
    public async Task PriorityAndTtl_FollowTheType(LoaiThongBao loai, UuTienPush uuTien, int ttlPhut)
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        await _kit.ThemThongBaoAsync(_tk, loai);

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var tin = Assert.Single(_kit.Sender.DaGui);
        Assert.Equal(uuTien, tin.UuTien);
        Assert.Equal(TimeSpan.FromMinutes(ttlPhut), tin.Ttl);
    }

    [Fact]
    public async Task RowsNotDue_OrNotPush_AreIgnored()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var chuaToi = await _kit.ThemThongBaoAsync(_tk, duKien: Now.AddMinutes(5));
        var local = await _kit.ThemThongBaoAsync(_tk, kenh: KenhThongBao.local);
        var hopThu = await _kit.ThemThongBaoAsync(_tk, kenh: KenhThongBao.chi_hop_thu);
        var daGui = await _kit.ThemThongBaoAsync(_tk, trangThai: TrangThaiGui.da_gui);

        Assert.Equal(0, await _kit.DayService().BLDayThongBaoChoGuiAsync());

        Assert.Empty(_kit.Sender.DaGui);
        Assert.Equal(TrangThaiGui.cho_gui, (await _kit.LayAsync(chuaToi)).trang_thai_gui);
        Assert.Equal(TrangThaiGui.cho_gui, (await _kit.LayAsync(local)).trang_thai_gui);
        Assert.Equal(TrangThaiGui.cho_gui, (await _kit.LayAsync(hopThu)).trang_thai_gui);
        Assert.Equal(TrangThaiGui.da_gui, (await _kit.LayAsync(daGui)).trang_thai_gui);
    }

    [Fact]
    public async Task TransientError_RetriesWithGrowingBackoff_ThenGivesUp()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = _ => new KetQuaPush(KetQuaDay.LoiTam, Loi: "UNAVAILABLE");
        _kit.Options.MaxAttempts = 3;
        _kit.Options.BackoffSeconds = 30;

        await _kit.DayService().BLDayThongBaoChoGuiAsync();
        var lan1 = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.cho_gui, lan1.trang_thai_gui);
        Assert.Equal(1, lan1.so_lan_thu);
        Assert.Equal(Now.AddSeconds(30), lan1.thoi_diem_thu_lai);

        // Chưa hết thời gian chờ thì chưa thử lại.
        Assert.Equal(0, await _kit.DayService().BLDayThongBaoChoGuiAsync());

        _kit.Time.Now = Now.AddSeconds(31);
        await _kit.DayService().BLDayThongBaoChoGuiAsync();
        var lan2 = await _kit.LayAsync(tb);
        Assert.Equal(2, lan2.so_lan_thu);
        Assert.Equal(Now.AddSeconds(31).AddSeconds(60), lan2.thoi_diem_thu_lai);

        _kit.Time.Now = Now.AddMinutes(10);
        await _kit.DayService().BLDayThongBaoChoGuiAsync();
        var lan3 = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.loi, lan3.trang_thai_gui);
        Assert.Equal(3, lan3.so_lan_thu);
        Assert.Equal(3, _kit.Sender.DaGui.Count);
    }

    [Fact]
    public async Task Backoff_IsCappedAtTheMaximum()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = _ => new KetQuaPush(KetQuaDay.LoiTam);
        _kit.Options.MaxAttempts = 20;
        _kit.Options.BackoffSeconds = 30;
        _kit.Options.MaxBackoffSeconds = 100;

        var cho = new List<double>();
        for (var i = 0; i < 6; i++)
        {
            await _kit.DayService().BLDayThongBaoChoGuiAsync();
            var hienTai = await _kit.LayAsync(tb);
            cho.Add((hienTai.thoi_diem_thu_lai!.Value - _kit.Time.Now).TotalSeconds);
            _kit.Time.Now = hienTai.thoi_diem_thu_lai!.Value;
        }

        Assert.Equal([30, 60, 100, 100, 100, 100], cho);
    }

    [Fact]
    public async Task DeadToken_IsCleared_AndDeviceDeactivated_WithoutRetry()
    {
        var chet = await _kit.ThemThietBiAsync(_tk, "token-chet");
        var song = await _kit.ThemThietBiAsync(_tk, "token-song");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = tin => tin.PushToken == "token-chet"
            ? new KetQuaPush(KetQuaDay.TokenChet, Loi: "UNREGISTERED")
            : new KetQuaPush(KetQuaDay.ThanhCong, "ok");

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var tbChet = (await _kit.ThietBis.DLGetByIdAsync(chet.Id, default))!;
        Assert.Null(tbChet.push_token);
        Assert.False(tbChet.dang_hoat_dong);
        Assert.True((await _kit.ThietBis.DLGetByIdAsync(song.Id, default))!.dang_hoat_dong);
        Assert.Equal(TrangThaiGui.da_gui, (await _kit.LayAsync(tb)).trang_thai_gui);
    }

    [Fact]
    public async Task OnlyDeadTokens_CancelTheRow_BecauseNoDeviceIsLeft()
    {
        await _kit.ThemThietBiAsync(_tk, "token-chet");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = _ => new KetQuaPush(KetQuaDay.TokenChet);

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        Assert.Equal(TrangThaiGui.huy, (await _kit.LayAsync(tb)).trang_thai_gui);
    }

    [Fact]
    public async Task ConfigError_FailsTheRowAtOnce_WithoutRetry()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = _ => new KetQuaPush(KetQuaDay.LoiCauHinh, Loi: "SENDER_ID_MISMATCH");

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var sau = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.loi, sau.trang_thai_gui);
        Assert.Single(_kit.Sender.DaGui);
    }

    [Fact]
    public async Task OneDeviceSucceedsAndOneFailsTemporarily_CountsAsSent()
    {
        await _kit.ThemThietBiAsync(_tk, "token-ok");
        await _kit.ThemThietBiAsync(_tk, "token-cham");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = tin => tin.PushToken == "token-cham"
            ? new KetQuaPush(KetQuaDay.LoiTam)
            : new KetQuaPush(KetQuaDay.ThanhCong, "ok");

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        Assert.Equal(TrangThaiGui.da_gui, (await _kit.LayAsync(tb)).trang_thai_gui);
    }

    [Fact]
    public async Task AccountWithoutDevices_CancelsTheRow_ButKeepsItInTheInbox()
    {
        var tb = await _kit.ThemThongBaoAsync(_tk);

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var sau = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.huy, sau.trang_thai_gui);
        Assert.Empty(_kit.Sender.DaGui);
        Assert.Single(await _kit.ThongBaos.DLFindInboxAsync(_tk, Now, false, 0, 10, default));
    }

    [Fact]
    public async Task TwoWorkers_CannotClaimTheSameRow()
    {
        var tb = await _kit.ThemThongBaoAsync(_tk);
        var daThay = (await _kit.ThongBaos.DLFindDueAsync(Now, Now.AddMinutes(-5), 10, default)).Single();

        Assert.True(await _kit.ThongBaos.DLTryClaimAsync(daThay, Now, default));
        Assert.False(await _kit.ThongBaos.DLTryClaimAsync(daThay, Now, default));
        Assert.Equal(TrangThaiGui.dang_gui, (await _kit.LayAsync(tb)).trang_thai_gui);
    }

    [Fact]
    public async Task StuckSending_IsReclaimedAfterTheLimit_ButRecentOneIsLeftAlone()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var treo = await _kit.ThemThongBaoAsync(_tk, trangThai: TrangThaiGui.dang_gui, nhan: Now.AddMinutes(-30));
        var moiNhan = await _kit.ThemThongBaoAsync(_tk, trangThai: TrangThaiGui.dang_gui, nhan: Now.AddMinutes(-1));

        var daXuLy = await _kit.DayService().BLDayThongBaoChoGuiAsync();

        Assert.Equal(1, daXuLy);
        Assert.Equal(TrangThaiGui.da_gui, (await _kit.LayAsync(treo)).trang_thai_gui);
        Assert.Equal(TrangThaiGui.dang_gui, (await _kit.LayAsync(moiNhan)).trang_thai_gui);
    }

    [Fact]
    public async Task SenderThrowing_IsTreatedAsTransient_NotLost()
    {
        await _kit.ThemThietBiAsync(_tk, "token-a");
        var tb = await _kit.ThemThongBaoAsync(_tk);
        _kit.Sender.KetQua = _ => throw new HttpRequestException("mạng đứt");

        await _kit.DayService().BLDayThongBaoChoGuiAsync();

        var sau = await _kit.LayAsync(tb);
        Assert.Equal(TrangThaiGui.cho_gui, sau.trang_thai_gui);
        Assert.Equal(1, sau.so_lan_thu);
    }
}
