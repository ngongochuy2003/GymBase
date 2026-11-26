using GymBase.BLBiz.BaiViet;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Biz;

public sealed class BaiVietFileTruyCapPolicyTests
{
    private static readonly Guid TacGia = Guid.NewGuid();
    private static readonly Guid NguoiXem = Guid.NewGuid();

    private readonly InMemoryStore<bai_viet> _store = new();
    private readonly InMemoryBaiVietRepository _repo;

    public BaiVietFileTruyCapPolicyTests() => _repo = new InMemoryBaiVietRepository(_store);

    private async Task<TepTinTruyCap> BaiVietCoTepAsync(CheDoHienThi cheDo, TrangThaiBaiViet trangThai = TrangThaiBaiViet.da_dang)
    {
        var bai = new bai_viet { tac_gia_id = TacGia, che_do_hien_thi = cheDo, trang_thai = trangThai, ngay_tao = DateTime.UtcNow };
        await _repo.DLInsertAsync(bai, default);
        return new TepTinTruyCap(Guid.NewGuid(), TacGia, bai.Id, "post");
    }

    private Task<IReadOnlySet<Guid>> HoiAsync(Guid nguoiXem, params TepTinTruyCap[] cacTep) =>
        new BaiVietFileTruyCapPolicy(_repo).BLKiemTruyCapAsync(nguoiXem, cacTep, default);

    [Theory]
    [InlineData(CheDoHienThi.cong_khai)]
    [InlineData(CheDoHienThi.nguoi_theo_doi)]
    public async Task PublicAndFollowersPosts_AreVisibleToAnyone(CheDoHienThi cheDo)
    {
        var tep = await BaiVietCoTepAsync(cheDo);

        Assert.Contains(tep.Id, await HoiAsync(NguoiXem, tep));
    }

    [Fact]
    public async Task PrivatePost_IsVisibleOnlyToAuthor()
    {
        var tep = await BaiVietCoTepAsync(CheDoHienThi.rieng_tu);

        Assert.DoesNotContain(tep.Id, await HoiAsync(NguoiXem, tep));
        Assert.Contains(tep.Id, await HoiAsync(TacGia, tep));
    }

    [Fact]
    public async Task HiddenPost_IsVisibleOnlyToAuthor()
    {
        var tep = await BaiVietCoTepAsync(CheDoHienThi.cong_khai, TrangThaiBaiViet.an);

        Assert.DoesNotContain(tep.Id, await HoiAsync(NguoiXem, tep));
        Assert.Contains(tep.Id, await HoiAsync(TacGia, tep));
    }

    [Fact]
    public async Task FileOfMissingPost_IsNotVisible()
    {
        var tep = new TepTinTruyCap(Guid.NewGuid(), TacGia, Guid.NewGuid(), "post");

        Assert.Empty(await HoiAsync(NguoiXem, tep));
    }

    [Fact]
    public async Task MixedBatch_ReturnsOnlyTheVisibleFiles()
    {
        var cong = await BaiVietCoTepAsync(CheDoHienThi.cong_khai);
        var rieng = await BaiVietCoTepAsync(CheDoHienThi.rieng_tu);

        Assert.Equal([cong.Id], (await HoiAsync(NguoiXem, cong, rieng)).ToArray());
    }
}
