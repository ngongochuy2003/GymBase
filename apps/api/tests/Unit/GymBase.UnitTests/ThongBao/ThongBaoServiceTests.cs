using GymBase.Base.Exceptions;
using GymBase.BLThongBao.BLThongBao;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.ThongBao;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

public sealed class ThongBaoServiceTests
{
    private static readonly DateTime Now = ThongBaoKit.Now;
    private readonly ThongBaoKit _kit = new();
    private readonly Guid _tk = Guid.NewGuid();

    private ThongBaoService Service() => new(_kit.ThongBaos, _kit.CaiDats, _kit.Time);

    private static TaoThongBaoRequest Req(Guid tk, string loai = "tin_nhan", string ma = "tin_nhan:1", DateTime? duKien = null) =>
        new(tk, loai, "Bạn có tin nhắn mới", "chi tiết", "tin_nhan", Guid.NewGuid(), ma, duKien);

    private async Task<thong_bao> LayAsync(Guid id) => (await _kit.ThongBaos.DLGetByIdAsync(id, default))!;

    [Theory]
    [InlineData("tin_nhan", KenhThongBao.push)]
    [InlineData("danh_gia", KenhThongBao.push)]
    [InlineData("ket_noi_pt", KenhThongBao.push)]
    [InlineData("nhac_an", KenhThongBao.local)]
    [InlineData("nhac_tap", KenhThongBao.local)]
    public async Task Create_ChoosesTheChannelByType(string loai, KenhThongBao kenh)
    {
        var id = await Service().BLTaoAsync(Req(_tk, loai));

        var tb = await LayAsync(id);
        Assert.Equal(kenh, tb.kenh);
        Assert.Equal(TrangThaiGui.cho_gui, tb.trang_thai_gui);
        Assert.Equal(Now, tb.thoi_diem_du_kien);
        Assert.Equal(Now, tb.ngay_tao);
        Assert.Null(tb.ngay_da_doc);
    }

    [Fact]
    public async Task Create_WithADisabledChannel_StillWritesTheRow_ButWithoutABell()
    {
        await _kit.CaiDats.DLInsertAsync(new cai_dat_thong_bao { tai_khoan_id = _tk, loai_thong_bao = LoaiThongBao.tin_nhan, bat = false, ngay_tao = Now }, default);

        var id = await Service().BLTaoAsync(Req(_tk));

        Assert.Equal(KenhThongBao.chi_hop_thu, (await LayAsync(id)).kenh);
        Assert.Equal(1, await Service().BLDemChuaDocAsync(_tk));
    }

    [Fact]
    public async Task Create_WithAnotherTypeDisabled_DoesNotAffectThisType()
    {
        await _kit.CaiDats.DLInsertAsync(new cai_dat_thong_bao { tai_khoan_id = _tk, loai_thong_bao = LoaiThongBao.danh_gia, bat = false, ngay_tao = Now }, default);

        var id = await Service().BLTaoAsync(Req(_tk, "tin_nhan"));

        Assert.Equal(KenhThongBao.push, (await LayAsync(id)).kenh);
    }

    [Fact]
    public async Task Create_WithTheSameDedupKey_ReturnsTheExistingRow()
    {
        var a = await Service().BLTaoAsync(Req(_tk, ma: "tin_nhan:7"));
        var b = await Service().BLTaoAsync(Req(_tk, ma: "tin_nhan:7"));
        var khac = await Service().BLTaoAsync(Req(Guid.NewGuid(), ma: "tin_nhan:7"));

        Assert.Equal(a, b);
        Assert.NotEqual(a, khac);
    }

    [Theory]
    [InlineData("khong_co_loai", "t", "ma")]
    [InlineData("tin_nhan", "", "ma")]
    [InlineData("tin_nhan", "t", "")]
    public async Task Create_RejectsBadInput(string loai, string tieuDe, string ma)
    {
        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            Service().BLTaoAsync(new TaoThongBaoRequest(_tk, loai, tieuDe, null, null, null, ma)));
    }

    [Fact]
    public async Task Inbox_ShowsOnlyOwnRowsThatAreDue_NewestFirst()
    {
        var cu = await Service().BLTaoAsync(Req(_tk, ma: "a", duKien: Now.AddHours(-2)));
        var moi = await Service().BLTaoAsync(Req(_tk, ma: "b", duKien: Now.AddHours(-1)));
        await Service().BLTaoAsync(Req(_tk, "nhac_tap", ma: "c", duKien: Now.AddHours(3)));
        await Service().BLTaoAsync(Req(Guid.NewGuid(), ma: "d"));

        var hopThu = await Service().BLLayHopThuAsync(_tk, false, 0, 20);

        Assert.Equal([moi, cu], hopThu.Data.Select(t => t.Id));
        Assert.False(hopThu.ConNua);
    }

    [Fact]
    public async Task Inbox_PagesWithOffset_AndTellsWhetherMoreRemain()
    {
        for (var i = 0; i < 5; i++)
        {
            await Service().BLTaoAsync(Req(_tk, ma: $"m{i}", duKien: Now.AddMinutes(-i)));
        }

        var trang1 = await Service().BLLayHopThuAsync(_tk, false, 0, 2);
        var trang2 = await Service().BLLayHopThuAsync(_tk, false, 2, 2);
        var trang3 = await Service().BLLayHopThuAsync(_tk, false, 4, 2);

        Assert.Equal(2, trang1.Data.Count);
        Assert.True(trang1.ConNua);
        Assert.Equal(2, trang2.Data.Count);
        Assert.True(trang2.ConNua);
        Assert.Single(trang3.Data);
        Assert.False(trang3.ConNua);
        Assert.Equal(5, trang1.Data.Concat(trang2.Data).Concat(trang3.Data).Select(t => t.Id).Distinct().Count());
    }

    [Fact]
    public async Task Inbox_CanFilterUnreadOnly_AndMarkReadIsOwnerOnly()
    {
        var a = await Service().BLTaoAsync(Req(_tk, ma: "a"));
        var b = await Service().BLTaoAsync(Req(_tk, ma: "b"));

        await Service().BLDanhDauDaDocAsync(_tk, a);

        var chuaDoc = await Service().BLLayHopThuAsync(_tk, true, 0, 20);
        Assert.Equal([b], chuaDoc.Data.Select(t => t.Id));
        Assert.Equal(1, await Service().BLDemChuaDocAsync(_tk));

        await Assert.ThrowsAsync<NotFoundException>(() => Service().BLDanhDauDaDocAsync(Guid.NewGuid(), b));
        await Assert.ThrowsAsync<NotFoundException>(() => Service().BLDanhDauDaDocAsync(_tk, Guid.NewGuid()));
    }

    [Fact]
    public async Task MarkRead_IsIdempotent_AndKeepsTheFirstReadTime()
    {
        var a = await Service().BLTaoAsync(Req(_tk));
        await Service().BLDanhDauDaDocAsync(_tk, a);
        _kit.Time.Now = Now.AddHours(1);

        await Service().BLDanhDauDaDocAsync(_tk, a);

        Assert.Equal(Now, (await LayAsync(a)).ngay_da_doc);
    }

    [Fact]
    public async Task MarkAllRead_OnlyTouchesTheCallersVisibleRows()
    {
        await Service().BLTaoAsync(Req(_tk, ma: "a"));
        await Service().BLTaoAsync(Req(_tk, ma: "b"));
        var henGio = await Service().BLTaoAsync(Req(_tk, "nhac_tap", ma: "c", duKien: Now.AddHours(2)));
        var nguoiKhac = await Service().BLTaoAsync(Req(Guid.NewGuid(), ma: "d"));

        var dem = await Service().BLDanhDauTatCaDaDocAsync(_tk);

        Assert.Equal(2, dem);
        Assert.Null((await LayAsync(henGio)).ngay_da_doc);
        Assert.Null((await LayAsync(nguoiKhac)).ngay_da_doc);
        Assert.Equal(0, await Service().BLDemChuaDocAsync(_tk));
    }

    [Fact]
    public async Task InboxItem_ExposesDetailForTheApp_ButNotInternalQueueFields()
    {
        await Service().BLTaoAsync(Req(_tk));

        var item = (await Service().BLLayHopThuAsync(_tk, false, 0, 20)).Data.Single();

        Assert.Equal("tin_nhan", item.LoaiThongBao);
        Assert.Equal("chi tiết", item.NoiDung);
        Assert.False(item.DaDoc);
        Assert.Null(typeof(ThongBaoDto).GetProperty("SoLanThu"));
        Assert.Null(typeof(ThongBaoDto).GetProperty("MaChongTrung"));
    }
}
