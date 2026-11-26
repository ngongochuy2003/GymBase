using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;

namespace GymBase.UnitTests.File;

public sealed class TepTinXuLyServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(Now);
    }

    private readonly MemoryFileStorage _storage = new();
    private readonly InMemoryTepTinRepository _repo = new(new InMemoryStore<tep_tin>());
    private readonly StorageOptions _options = new();

    private TepTinXuLyService Service() => new(
        _repo,
        new MemoryStorageResolver(_storage),
        new SkiaImageProcessor(),
        Options.Create(_options),
        new FixedTimeProvider(),
        NullLogger<TepTinXuLyService>.Instance);

    private async Task<tep_tin> ThemAsync(
        byte[] data, string mime = "image/jpeg", TrangThaiXuLyTepTin xuLy = TrangThaiXuLyTepTin.cho_xu_ly, DateTime? batDau = null)
    {
        var id = Guid.CreateVersion7();
        var tep = new tep_tin
        {
            Id = id,
            nguoi_tai_len_id = Guid.NewGuid(),
            muc_dich = "post",
            s3_bucket = "gymbase-files",
            s3_key = $"post/2026/11/{id}.jpg",
            loai_mime = mime,
            kich_thuoc_byte = data.Length,
            che_do_truy_cap = CheDoTruyCap.cong_khai,
            trang_thai = TrangThaiTepTin.da_tai_len,
            trang_thai_xu_ly = xuLy,
            ngay_bat_dau_xu_ly = batDau,
            ngay_tao = Now.AddMinutes(-5),
            ngay_xac_nhan = Now.AddMinutes(-5),
        };
        await _repo.DLInsertAsync(tep, default);
        _storage.Objects[tep.s3_key] = (data, mime);
        return tep;
    }

    private async Task<tep_tin> LayAsync(tep_tin tep) => (await _repo.DLGetByIdAsync(tep.Id, default))!;

    [Fact]
    public async Task PendingImage_IsCleanedInPlace_AndStoresBlurhashAndNewSize()
    {
        var goc = MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 6);
        var tep = await ThemAsync(goc);

        var daXuLy = await Service().BLXuLyTepChoXuLyAsync();

        Assert.Equal(1, daXuLy);
        var sau = await LayAsync(tep);
        Assert.Equal(TrangThaiXuLyTepTin.xong, sau.trang_thai_xu_ly);
        Assert.Equal(TrangThaiTepTin.da_tai_len, sau.trang_thai);
        Assert.False(string.IsNullOrEmpty(sau.blurhash));
        var luu = _storage.Objects[tep.s3_key].Data;
        Assert.False(MauAnh.CoChuoi(luu, "GPS-LAT"));
        Assert.Equal(luu.Length, sau.kich_thuoc_byte);
        Assert.Equal("image/jpeg", _storage.Objects[tep.s3_key].ContentType);
    }

    [Fact]
    public async Task OversizedImage_IsRejected_ObjectDeleted_AndFileMarkedError()
    {
        var tep = await ThemAsync(MauAnh.Tao(8193, 2, SKEncodedImageFormat.Png), "image/png");

        await Service().BLXuLyTepChoXuLyAsync();

        var sau = await LayAsync(tep);
        Assert.Equal(TrangThaiTepTin.loi, sau.trang_thai);
        Assert.Equal(TrangThaiXuLyTepTin.loi, sau.trang_thai_xu_ly);
        Assert.False(_storage.Objects.ContainsKey(tep.s3_key));
    }

    [Fact]
    public async Task CorruptImage_IsRejected()
    {
        var tep = await ThemAsync([0xFF, 0xD8, 0xFF, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);

        await Service().BLXuLyTepChoXuLyAsync();

        Assert.Equal(TrangThaiTepTin.loi, (await LayAsync(tep)).trang_thai);
        Assert.False(_storage.Objects.ContainsKey(tep.s3_key));
    }

    [Fact]
    public async Task FilesThatNeedNoProcessing_OrAlreadyDone_AreIgnored()
    {
        var khongCan = await ThemAsync(MauAnh.Tao(8, 8), xuLy: TrangThaiXuLyTepTin.khong_can);
        var xong = await ThemAsync(MauAnh.Tao(8, 8), xuLy: TrangThaiXuLyTepTin.xong);

        Assert.Equal(0, await Service().BLXuLyTepChoXuLyAsync());

        Assert.Equal(TrangThaiXuLyTepTin.khong_can, (await LayAsync(khongCan)).trang_thai_xu_ly);
        Assert.Equal(TrangThaiXuLyTepTin.xong, (await LayAsync(xong)).trang_thai_xu_ly);
    }

    [Fact]
    public async Task TwoWorkers_CannotClaimTheSameFile()
    {
        var tep = await ThemAsync(MauAnh.Tao(8, 8));
        var daThay = (await _repo.DLFindPendingProcessingAsync(Now, 10, default)).Single();

        Assert.True(await _repo.DLTryClaimProcessingAsync(daThay, Now, default));
        Assert.False(await _repo.DLTryClaimProcessingAsync(daThay, Now, default));
        Assert.Equal(TrangThaiXuLyTepTin.dang_xu_ly, (await LayAsync(tep)).trang_thai_xu_ly);
    }

    [Fact]
    public async Task StuckProcessing_IsReclaimedAfterTheLimit_ButRecentOneIsLeftAlone()
    {
        var treo = await ThemAsync(MauAnh.Tao(8, 8), xuLy: TrangThaiXuLyTepTin.dang_xu_ly, batDau: Now.AddMinutes(-30));
        var moiNhan = await ThemAsync(MauAnh.Tao(8, 8), xuLy: TrangThaiXuLyTepTin.dang_xu_ly, batDau: Now.AddMinutes(-2));

        var daXuLy = await Service().BLXuLyTepChoXuLyAsync();

        Assert.Equal(1, daXuLy);
        Assert.Equal(TrangThaiXuLyTepTin.xong, (await LayAsync(treo)).trang_thai_xu_ly);
        Assert.Equal(TrangThaiXuLyTepTin.dang_xu_ly, (await LayAsync(moiNhan)).trang_thai_xu_ly);
    }

    [Theory]
    [InlineData("image/jpeg", TrangThaiXuLyTepTin.cho_xu_ly)]
    [InlineData("image/png", TrangThaiXuLyTepTin.cho_xu_ly)]
    [InlineData("image/webp", TrangThaiXuLyTepTin.cho_xu_ly)]
    [InlineData("image/heic", TrangThaiXuLyTepTin.khong_can)]
    [InlineData("video/mp4", TrangThaiXuLyTepTin.khong_can)]
    [InlineData("application/pdf", TrangThaiXuLyTepTin.khong_can)]
    public void TrangThaiXuLyBanDau_DependsOnMime(string mime, TrangThaiXuLyTepTin expected) =>
        Assert.Equal(expected, FilePolicy.TrangThaiXuLyBanDau(mime));

    [Fact]
    public async Task Confirm_QueuesImagesForProcessing_AndLeavesPdfAlone()
    {
        var repo = new InMemoryTepTinRepository(new InMemoryStore<tep_tin>());
        var storage = new MemoryFileStorage();
        var svc = new TepTinService(
            new MemoryStorageResolver(storage), repo, [], Options.Create(new StorageOptions { DefaultProvider = "MINIO", BucketName = "b" }),
            TimeProvider.System, NullLogger<TepTinService>.Instance);
        var user = Guid.NewGuid();

        var anh = await svc.BLXinUploadAsync(user, new XinUploadRequestDto("post", "image/jpeg", 16, "a.jpg"), default);
        storage.Objects[FilePolicy.KeyTam(anh.FileId)] = (MauByte.Jpeg, "image/jpeg");
        await svc.BLXacNhanUploadAsync(user, anh.FileId, default);

        var pdf = await svc.BLXinUploadAsync(user, new XinUploadRequestDto("chung_chi_pt", "application/pdf", 16, "c.pdf"), default);
        storage.Objects[FilePolicy.KeyTam(pdf.FileId)] = (MauByte.Pdf, "application/pdf");
        await svc.BLXacNhanUploadAsync(user, pdf.FileId, default);

        Assert.Equal(TrangThaiXuLyTepTin.cho_xu_ly, (await repo.DLGetByIdAsync(anh.FileId, default))!.trang_thai_xu_ly);
        Assert.Equal(TrangThaiXuLyTepTin.khong_can, (await repo.DLGetByIdAsync(pdf.FileId, default))!.trang_thai_xu_ly);
    }
}
