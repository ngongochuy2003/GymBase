using GymBase.Base.Exceptions;
using GymBase.BLThongBao.BLThietBi;
using GymBase.Entities.Biz.Enums;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

public sealed class ThietBiServiceTests
{
    private static readonly DateTime Now = ThongBaoKit.Now;
    private readonly ThongBaoKit _kit = new();
    private readonly Guid _tk = Guid.NewGuid();

    private ThietBiService Service() => new(_kit.ThietBis, _kit.Time);

    private static DangKyThietBiDto Dto(string ma = "may-1", string nenTang = "android", string? token = "tok-1", string? muiGio = "Asia/Ho_Chi_Minh") =>
        new(ma, nenTang, token, muiGio);

    [Fact]
    public async Task Register_CreatesActiveDevice_WithTokenAndTimeZone()
    {
        await Service().BLDangKyAsync(_tk, Dto());

        var tb = (await _kit.ThietBis.DLFindByMaThietBiAsync(_tk, "may-1", default))!;
        Assert.Equal(NenTang.android, tb.nen_tang);
        Assert.Equal("tok-1", tb.push_token);
        Assert.Equal("Asia/Ho_Chi_Minh", tb.mui_gio);
        Assert.True(tb.dang_hoat_dong);
        Assert.Equal(Now, tb.ngay_hoat_dong_cuoi);
    }

    [Fact]
    public async Task Register_Twice_UpdatesTheSameRow_Upsert()
    {
        await Service().BLDangKyAsync(_tk, Dto());
        _kit.Time.Now = Now.AddHours(1);

        await Service().BLDangKyAsync(_tk, Dto(token: "tok-2", muiGio: "Asia/Bangkok"));

        var tb = Assert.Single(await _kit.ThietBis.DLFindActiveWithTokenAsync(_tk, default));
        Assert.Equal("tok-2", tb.push_token);
        Assert.Equal("Asia/Bangkok", tb.mui_gio);
        Assert.Equal(Now.AddHours(1), tb.ngay_hoat_dong_cuoi);
    }

    [Fact]
    public async Task Register_ReactivatesADeviceThatWasSignedOut()
    {
        await Service().BLDangKyAsync(_tk, Dto());
        await Service().BLGoAsync(_tk, "may-1");

        await Service().BLDangKyAsync(_tk, Dto(token: "tok-moi"));

        var tb = (await _kit.ThietBis.DLFindByMaThietBiAsync(_tk, "may-1", default))!;
        Assert.True(tb.dang_hoat_dong);
        Assert.Equal("tok-moi", tb.push_token);
    }

    [Fact]
    public async Task Register_MovesTheTokenAwayFromAnotherAccount()
    {
        var nguoiTruoc = Guid.NewGuid();
        await Service().BLDangKyAsync(nguoiTruoc, Dto(ma: "may-cu", token: "tok-chung"));

        await Service().BLDangKyAsync(_tk, Dto(ma: "may-moi", token: "tok-chung"));

        var cu = (await _kit.ThietBis.DLFindByMaThietBiAsync(nguoiTruoc, "may-cu", default))!;
        Assert.Null(cu.push_token);
        Assert.False(cu.dang_hoat_dong);
        Assert.Equal("tok-chung", (await _kit.ThietBis.DLFindByMaThietBiAsync(_tk, "may-moi", default))!.push_token);
        Assert.Empty((await _kit.ThietBis.DLFindActiveWithTokenAsync(nguoiTruoc, default)));
    }

    [Fact]
    public async Task Register_WithoutToken_IsAllowed_AndKeepsDeviceKnown()
    {
        await Service().BLDangKyAsync(_tk, Dto(token: null));

        var tb = (await _kit.ThietBis.DLFindByMaThietBiAsync(_tk, "may-1", default))!;
        Assert.Null(tb.push_token);
        Assert.True(tb.dang_hoat_dong);
    }

    [Fact]
    public async Task SignOut_ClearsTheTokenAndDeactivates_AndIsIdempotent()
    {
        await Service().BLDangKyAsync(_tk, Dto());

        await Service().BLGoAsync(_tk, "may-1");
        await Service().BLGoAsync(_tk, "may-1");
        await Service().BLGoAsync(_tk, "khong-co");

        var tb = (await _kit.ThietBis.DLFindByMaThietBiAsync(_tk, "may-1", default))!;
        Assert.Null(tb.push_token);
        Assert.False(tb.dang_hoat_dong);
    }

    [Fact]
    public async Task SignOut_OnlyAffectsTheCallersOwnDevice()
    {
        var khac = Guid.NewGuid();
        await Service().BLDangKyAsync(khac, Dto(token: "tok-khac"));

        await Service().BLGoAsync(_tk, "may-1");

        Assert.True((await _kit.ThietBis.DLFindByMaThietBiAsync(khac, "may-1", default))!.dang_hoat_dong);
    }

    [Theory]
    [InlineData("", "android", "ma_thiet_bi")]
    [InlineData("may-1", "windows", "nen_tang")]
    [InlineData("may-1", "android", "push_token", 513)]
    [InlineData("may-1", "android", "mui_gio", 0, "khong hop le!")]
    public async Task Register_RejectsBadInput(string ma, string nenTang, string truong, int doDaiToken = 0, string? muiGio = "Asia/Ho_Chi_Minh")
    {
        var token = doDaiToken > 0 ? new string('x', doDaiToken) : "tok";

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() =>
            Service().BLDangKyAsync(_tk, new DangKyThietBiDto(ma, nenTang, token, muiGio)));

        Assert.Contains(truong, ex.Errors!.Keys);
    }
}
