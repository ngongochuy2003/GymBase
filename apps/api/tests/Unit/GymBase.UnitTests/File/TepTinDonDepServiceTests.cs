using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLStorage;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.File;

public sealed class TepTinDonDepServiceTests
{
    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
    }

    /// <summary>Kho xóa lỗi với một key cho trước, để kiểm lượt dọn vẫn tiếp tục và giữ dòng của tệp đó.</summary>
    private sealed class FailingDeleteStorage(MemoryFileStorage inner, string failKey) : IFileStorage
    {
        public string ProviderName => inner.ProviderName;
        public bool SupportsPresign => true;
        public Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default) => inner.SaveAsync(key, content, contentType, ct);
        public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) => inner.OpenReadAsync(key, ct);
        public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default) => inner.GetInfoAsync(key, ct);
        public PresignedUpload CreatePresignedUpload(string key, string contentType, long maxBytes, TimeSpan expiry) => inner.CreatePresignedUpload(key, contentType, maxBytes, expiry);
        public string CreatePresignedGetUrl(string key, TimeSpan expiry) => inner.CreatePresignedGetUrl(key, expiry);
        public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct = default) => inner.CopyAsync(sourceKey, destinationKey, ct);
        public Task<byte[]> ReadHeadAsync(string key, int count, CancellationToken ct = default) => inner.ReadHeadAsync(key, count, ct);

        public Task DeleteAsync(string key, CancellationToken ct = default) =>
            key == failKey ? throw new IOException("Kho không phản hồi") : inner.DeleteAsync(key, ct);
    }

    private static readonly DateTime Now = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly MemoryFileStorage _storage = new();
    private readonly InMemoryTepTinRepository _repo = new(new InMemoryStore<tep_tin>());
    private readonly StorageOptions _options = new();

    private TepTinDonDepService Service(IFileStorage? storage = null) => new(
        _repo,
        new MemoryStorageResolver(_storage) is var resolver && storage == null ? resolver : new FixedResolver(storage),
        Options.Create(_options),
        new FixedTimeProvider(Now),
        NullLogger<TepTinDonDepService>.Instance);

    private sealed class FixedResolver(IFileStorage storage) : IStorageResolver
    {
        public IFileStorage Resolve(string providerName) => storage;
        public IFileStorage GetDefault() => storage;
    }

    private async Task<tep_tin> ThemAsync(TrangThaiTepTin trangThai, TimeSpan tuoi, bool coObjectChinh = false)
    {
        var id = Guid.CreateVersion7();
        var tep = new tep_tin
        {
            Id = id,
            nguoi_tai_len_id = Guid.NewGuid(),
            muc_dich = "post",
            s3_bucket = "gymbase-files",
            s3_key = $"post/2026/11/{id}.jpg",
            loai_mime = "image/jpeg",
            kich_thuoc_byte = 10,
            che_do_truy_cap = CheDoTruyCap.cong_khai,
            trang_thai = trangThai,
            ngay_tao = Now - tuoi,
            ngay_xac_nhan = trangThai == TrangThaiTepTin.cho_tai_len ? null : Now - tuoi,
        };
        await _repo.DLInsertAsync(tep, default);
        _storage.Objects[FilePolicy.KeyTam(id)] = ([1], "image/jpeg");
        if (coObjectChinh)
        {
            _storage.Objects[tep.s3_key] = ([1], "image/jpeg");
        }

        return tep;
    }

    private async Task<bool> ConTonTaiAsync(tep_tin tep) => await _repo.DLGetByIdAsync(tep.Id, default) != null;

    [Fact]
    public async Task PendingUpload_OlderThanLimit_IsDeletedWithItsObjects_NewerIsKept()
    {
        var cu = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromMinutes(61));
        var moi = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromMinutes(30));

        var daXoa = await Service().BLDonTepMoCoiAsync();

        Assert.Equal(1, daXoa);
        Assert.False(await ConTonTaiAsync(cu));
        Assert.False(_storage.Objects.ContainsKey(FilePolicy.KeyTam(cu.Id)));
        Assert.True(await ConTonTaiAsync(moi));
        Assert.True(_storage.Objects.ContainsKey(FilePolicy.KeyTam(moi.Id)));
    }

    [Fact]
    public async Task FailedUpload_OlderThanLimit_IsDeleted()
    {
        var loi = await ThemAsync(TrangThaiTepTin.loi, TimeSpan.FromMinutes(90));

        await Service().BLDonTepMoCoiAsync();

        Assert.False(await ConTonTaiAsync(loi));
    }

    [Fact]
    public async Task UnlinkedUploadedFile_OlderThanOneDay_IsDeletedIncludingFinalObject()
    {
        var cu = await ThemAsync(TrangThaiTepTin.da_tai_len, TimeSpan.FromHours(25), coObjectChinh: true);
        var moi = await ThemAsync(TrangThaiTepTin.da_tai_len, TimeSpan.FromHours(23), coObjectChinh: true);

        await Service().BLDonTepMoCoiAsync();

        Assert.False(await ConTonTaiAsync(cu));
        Assert.False(_storage.Objects.ContainsKey(cu.s3_key));
        Assert.True(await ConTonTaiAsync(moi));
        Assert.True(_storage.Objects.ContainsKey(moi.s3_key));
    }

    [Fact]
    public async Task LinkedFile_IsNeverDeleted_EvenWhenVeryOld()
    {
        var lienKet = await ThemAsync(TrangThaiTepTin.da_lien_ket, TimeSpan.FromDays(365), coObjectChinh: true);

        var daXoa = await Service().BLDonTepMoCoiAsync();

        Assert.Equal(0, daXoa);
        Assert.True(await ConTonTaiAsync(lienKet));
        Assert.True(_storage.Objects.ContainsKey(lienKet.s3_key));
    }

    [Fact]
    public async Task WhenOneObjectCannotBeDeleted_ItsRowIsKeptForRetryAndOthersAreStillCleaned()
    {
        var hong = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromHours(3));
        var tot = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromHours(2));

        var daXoa = await Service(new FailingDeleteStorage(_storage, FilePolicy.KeyTam(hong.Id))).BLDonTepMoCoiAsync();

        Assert.Equal(1, daXoa);
        Assert.True(await ConTonTaiAsync(hong));
        Assert.False(await ConTonTaiAsync(tot));

        // Lượt sau kho đã ổn: dọn nốt.
        Assert.Equal(1, await Service().BLDonTepMoCoiAsync());
        Assert.False(await ConTonTaiAsync(hong));
    }

    [Fact]
    public async Task EachRun_HandlesAtMostBatchSizeFiles_OldestFirst()
    {
        _options.CleanupBatchSize = 2;
        var a = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromHours(5));
        var b = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromHours(4));
        var c = await ThemAsync(TrangThaiTepTin.cho_tai_len, TimeSpan.FromHours(3));

        Assert.Equal(2, await Service().BLDonTepMoCoiAsync());

        Assert.False(await ConTonTaiAsync(a));
        Assert.False(await ConTonTaiAsync(b));
        Assert.True(await ConTonTaiAsync(c));
    }
}
