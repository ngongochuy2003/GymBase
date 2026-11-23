using GymBase.Base.Exceptions;
using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLStorage;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using GymBase.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.File;

/// <summary>Kho giả trong bộ nhớ, ký URL giả; đủ để test luồng xin upload, confirm và cấp URL đọc.</summary>
internal sealed class MemoryFileStorage : IFileStorage
{
    public Dictionary<string, (byte[] Data, string ContentType)> Objects { get; } = new();

    public string ProviderName => "MINIO";
    public bool SupportsPresign => true;

    public Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        Objects[key] = (ms.ToArray(), contentType);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream(Objects[key].Data));

    public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(Objects.TryGetValue(key, out var o) ? new StoredObjectInfo(o.Data.Length, o.ContentType) : null);

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        Objects.Remove(key);
        return Task.CompletedTask;
    }

    public PresignedUpload CreatePresignedUpload(string key, string contentType, long maxBytes, TimeSpan expiry) =>
        new("https://minio.test/upload", new Dictionary<string, string> { ["key"] = key, ["Content-Type"] = contentType, ["max"] = maxBytes.ToString() });

    public string CreatePresignedGetUrl(string key, TimeSpan expiry) => $"https://minio.test/get/{key}?exp={(int)expiry.TotalMinutes}";

    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct = default)
    {
        Objects[destinationKey] = Objects[sourceKey];
        return Task.CompletedTask;
    }

    public Task<byte[]> ReadHeadAsync(string key, int count, CancellationToken ct = default) =>
        Task.FromResult(Objects[key].Data.Take(count).ToArray());
}

internal sealed class MemoryStorageResolver(MemoryFileStorage storage) : IStorageResolver
{
    public IFileStorage Resolve(string providerName) => storage;
    public IFileStorage GetDefault() => storage;
}

internal sealed class FakeTruyCapPolicy(string loai, Func<Guid, IReadOnlyCollection<TepTinTruyCap>, IReadOnlySet<Guid>> rule) : IFileTruyCapPolicy
{
    public int SoLanGoi { get; private set; }
    public string LoaiDoiTuong => loai;

    public Task<IReadOnlySet<Guid>> BLKiemTruyCapAsync(Guid nguoiXemId, IReadOnlyCollection<TepTinTruyCap> cacTep, CancellationToken ct = default)
    {
        SoLanGoi++;
        return Task.FromResult(rule(nguoiXemId, cacTep));
    }
}

internal static class MauByte
{
    public static byte[] Jpeg { get; } = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1, 1, 0, 0, 1];

    public static byte[] Pdf { get; } = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7 mau tai lieu");

    /// <summary>Header ISO-BMFF: 4 byte độ dài, "ftyp", rồi brand 4 ký tự.</summary>
    public static byte[] Ftyp(string brand) =>
        [0, 0, 0, 0x18, .. System.Text.Encoding.ASCII.GetBytes("ftyp"), .. System.Text.Encoding.ASCII.GetBytes(brand), 0, 0, 0, 0];

    public static byte[] Riff(string loai) =>
        [.. System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 0, 0, 0, .. System.Text.Encoding.ASCII.GetBytes(loai)];
}

public class TepTinServiceTests
{
    private static readonly byte[] Jpeg = MauByte.Jpeg;
    private static readonly byte[] Pdf = MauByte.Pdf;

    private readonly MemoryFileStorage _storage = new();
    private readonly InMemoryTepTinRepository _repo = new(new InMemoryStore<tep_tin>());
    private readonly StorageOptions _options = new() { DefaultProvider = "MINIO", BucketName = "gymbase-files" };
    private readonly Guid _userId = Guid.NewGuid();

    private TepTinService Service(params IFileTruyCapPolicy[] policies) =>
        new(new MemoryStorageResolver(_storage), _repo, policies, Options.Create(_options), TimeProvider.System, NullLogger<TepTinService>.Instance);

    private static XinUploadRequestDto Xin(string muc, string mime, long size, string? ten = "a.jpg") => new(muc, mime, size, ten);

    private async Task<Guid> UploadVaConfirmAsync(TepTinService svc, Guid user, string muc, string mime, byte[] data)
    {
        var xin = await svc.BLXinUploadAsync(user, Xin(muc, mime, data.Length), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (data, mime);
        await svc.BLXacNhanUploadAsync(user, xin.FileId, default);
        return xin.FileId;
    }

    [Fact]
    public async Task XinUpload_ValidImage_CreatesPendingRowAndSignsUploadFormToTmpKey()
    {
        var result = await Service().BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 1000), default);

        var entity = await _repo.DLGetByIdAsync(result.FileId, default);
        Assert.NotNull(entity);
        Assert.Equal(TrangThaiTepTin.cho_tai_len, entity!.trang_thai);
        Assert.Equal(_userId, entity.nguoi_tai_len_id);
        Assert.Equal(CheDoTruyCap.cong_khai, entity.che_do_truy_cap);
        Assert.StartsWith("avatar/", entity.s3_key);
        Assert.Equal("POST", result.Upload.Method);
        Assert.Equal("file", result.Upload.FileField);
        Assert.Equal($"tmp/{result.FileId}", result.Upload.Fields["key"]);
        Assert.Equal("image/jpeg", result.Upload.Fields["Content-Type"]);
        Assert.Equal((10 * 1024 * 1024).ToString(), result.Upload.Fields["max"]);
    }

    [Theory]
    [InlineData("khong_co", "image/jpeg", 100, FileErrorCodes.PurposeInvalid)]
    [InlineData("avatar", "video/mp4", 100, FileErrorCodes.TypeNotAllowed)]
    [InlineData("avatar", "application/pdf", 100, FileErrorCodes.TypeNotAllowed)]
    [InlineData("chung_chi_pt", "image/jpeg", 100, FileErrorCodes.TypeNotAllowed)]
    [InlineData("avatar", "image/jpeg", 0, FileErrorCodes.TooLarge)]
    [InlineData("avatar", "image/jpeg", 10 * 1024 * 1024 + 1, FileErrorCodes.TooLarge)]
    [InlineData("post", "video/mp4", 100L * 1024 * 1024 + 1, FileErrorCodes.TooLarge)]
    public async Task XinUpload_InvalidRequest_ThrowsValidationWithCode(string muc, string mime, long size, string code)
    {
        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => Service().BLXinUploadAsync(_userId, Xin(muc, mime, size), default));
        Assert.Equal(code, ex.Code);
    }

    [Fact]
    public async Task XinUpload_VideoUpTo100MB_IsAllowedForPost()
    {
        var result = await Service().BLXinUploadAsync(_userId, Xin("post", "video/mp4", 100L * 1024 * 1024, "v.mp4"), default);
        Assert.NotEqual(Guid.Empty, result.FileId);
    }

    [Fact]
    public async Task XinUpload_MimeWithParametersAndUppercase_IsNormalized()
    {
        var result = await Service().BLXinUploadAsync(_userId, Xin("avatar", "Image/JPEG; charset=binary", 10), default);
        Assert.Equal("image/jpeg", (await _repo.DLGetByIdAsync(result.FileId, default))!.loai_mime);
    }

    [Fact]
    public async Task XinUpload_TooManyPending_ThrowsQuotaExceeded()
    {
        _options.MaxPendingUploads = 2;
        var svc = Service();
        await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default);
        await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default));
        Assert.Equal(FileErrorCodes.QuotaExceeded, ex.Code);
    }

    [Fact]
    public async Task XinUpload_DailyImageCountExceeded_ThrowsQuotaExceeded_ButOtherUserIsUnaffected()
    {
        _options.MaxImagesPerDay = 2;
        var svc = Service();
        await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default);
        await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default);

        await Assert.ThrowsAsync<BusinessRuleException>(() => svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default));
        await svc.BLXinUploadAsync(Guid.NewGuid(), Xin("avatar", "image/jpeg", 10), default);
    }

    [Fact]
    public async Task XinUpload_DailyBytesExceeded_ThrowsQuotaExceeded()
    {
        _options.MaxBytesPerDay = 1500;
        var svc = Service();
        await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 1000), default);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 600), default));
        Assert.Equal(FileErrorCodes.QuotaExceeded, ex.Code);
    }

    [Fact]
    public async Task XacNhan_ValidObject_CopiesToFinalKeyRemovesTmpAndMarksUploaded()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Jpeg.Length), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Jpeg, "image/jpeg");

        var result = await svc.BLXacNhanUploadAsync(_userId, xin.FileId, default);

        var entity = (await _repo.DLGetByIdAsync(xin.FileId, default))!;
        Assert.Equal(TrangThaiTepTin.da_tai_len, entity.trang_thai);
        Assert.NotNull(entity.ngay_xac_nhan);
        Assert.Equal("da_tai_len", result.TrangThai);
        Assert.True(_storage.Objects.ContainsKey(entity.s3_key));
        Assert.False(_storage.Objects.ContainsKey($"tmp/{xin.FileId}"));
    }

    [Fact]
    public async Task XacNhan_RecordsActualSizeFromStorage_NotTheEstimate()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 1), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Jpeg, "image/jpeg");

        var result = await svc.BLXacNhanUploadAsync(_userId, xin.FileId, default);

        Assert.Equal(Jpeg.Length, result.KichThuocByte);
        Assert.Equal(Jpeg.Length, (await _repo.DLGetByIdAsync(xin.FileId, default))!.kich_thuoc_byte);
    }

    [Fact]
    public async Task XacNhan_EmptyObject_IsRejected()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 10), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = ([], "image/jpeg");

        await Assert.ThrowsAsync<ValidationFailedException>(() => svc.BLXacNhanUploadAsync(_userId, xin.FileId, default));
    }

    [Fact]
    public async Task XacNhan_CalledTwice_IsIdempotent()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "avatar", "image/jpeg", Jpeg);

        var again = await svc.BLXacNhanUploadAsync(_userId, id, default);

        Assert.Equal("da_tai_len", again.TrangThai);
    }

    [Fact]
    public async Task XacNhan_ObjectMissing_ThrowsNotUploadedAndKeepsPending()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Jpeg.Length), default);

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => svc.BLXacNhanUploadAsync(_userId, xin.FileId, default));

        Assert.Equal(FileErrorCodes.NotUploaded, ex.Code);
        Assert.Equal(TrangThaiTepTin.cho_tai_len, (await _repo.DLGetByIdAsync(xin.FileId, default))!.trang_thai);
    }

    [Fact]
    public async Task XacNhan_ActualSizeAboveMaximum_MarksErrorAndDeletesTmp()
    {
        _options.MaxImageBytes = 8;
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", 5), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Jpeg, "image/jpeg");

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => svc.BLXacNhanUploadAsync(_userId, xin.FileId, default));

        Assert.Equal(FileErrorCodes.ContentMismatch, ex.Code);
        Assert.Equal(TrangThaiTepTin.loi, (await _repo.DLGetByIdAsync(xin.FileId, default))!.trang_thai);
        Assert.False(_storage.Objects.ContainsKey($"tmp/{xin.FileId}"));
    }

    [Fact]
    public async Task XacNhan_ContentTypeDiffersFromDeclared_IsRejected()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Jpeg.Length), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Jpeg, "image/png");

        await Assert.ThrowsAsync<ValidationFailedException>(() => svc.BLXacNhanUploadAsync(_userId, xin.FileId, default));
    }

    [Fact]
    public async Task XacNhan_PdfLabelledAsJpeg_IsRejectedByMagicBytes()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Pdf.Length), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Pdf, "image/jpeg");

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => svc.BLXacNhanUploadAsync(_userId, xin.FileId, default));

        Assert.Equal(FileErrorCodes.ContentMismatch, ex.Code);
        Assert.Equal(TrangThaiTepTin.loi, (await _repo.DLGetByIdAsync(xin.FileId, default))!.trang_thai);
    }

    [Fact]
    public async Task XacNhan_OtherUsersFile_ThrowsNotFound()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Jpeg.Length), default);
        _storage.Objects[$"tmp/{xin.FileId}"] = (Jpeg, "image/jpeg");

        await Assert.ThrowsAsync<NotFoundException>(() => svc.BLXacNhanUploadAsync(Guid.NewGuid(), xin.FileId, default));
    }

    [Fact]
    public async Task XacNhan_AfterConfirm_OverwritingTmpKeyDoesNotChangeFinalObject()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "avatar", "image/jpeg", Jpeg);
        var key = (await _repo.DLGetByIdAsync(id, default))!.s3_key;

        _storage.Objects[$"tmp/{id}"] = (Pdf, "application/pdf");

        Assert.Equal(Jpeg, _storage.Objects[key].Data);
    }

    [Fact]
    public async Task LayUrl_Owner_GetsPrivateUrlWithShortExpiry()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "progress", "image/jpeg", Jpeg);

        var url = await svc.BLLayUrlAsync(_userId, id, default);

        Assert.Contains("exp=5", url.Url);
    }

    [Fact]
    public async Task LayUrl_PendingFile_IsNotFound_EvenForOwner()
    {
        var svc = Service();
        var xin = await svc.BLXinUploadAsync(_userId, Xin("avatar", "image/jpeg", Jpeg.Length), default);

        await Assert.ThrowsAsync<NotFoundException>(() => svc.BLLayUrlAsync(_userId, xin.FileId, default));
    }

    [Fact]
    public async Task LayUrl_OtherUser_UnlinkedPublicFile_IsNotFound()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "post", "image/jpeg", Jpeg);

        await Assert.ThrowsAsync<NotFoundException>(() => svc.BLLayUrlAsync(Guid.NewGuid(), id, default));
    }

    [Fact]
    public async Task LayUrl_OtherUser_LinkedPublicFileWithoutPolicy_GetsUrlWithPublicExpiry()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "post", "image/jpeg", Jpeg);
        await Lien(id, LoaiDoiTuongTepTin.bai_viet);

        var url = await svc.BLLayUrlAsync(Guid.NewGuid(), id, default);

        Assert.Contains("exp=15", url.Url);
    }

    [Fact]
    public async Task LayUrl_OtherUser_LinkedPrivateFileWithoutPolicy_IsNotFound()
    {
        var svc = Service();
        var id = await UploadVaConfirmAsync(svc, _userId, "progress", "image/jpeg", Jpeg);
        await Lien(id, LoaiDoiTuongTepTin.nhat_ky_hoat_dong);

        await Assert.ThrowsAsync<NotFoundException>(() => svc.BLLayUrlAsync(Guid.NewGuid(), id, default));
    }

    [Fact]
    public async Task LayUrls_UsesPolicyOncePerObjectTypeAndOnlyReturnsAllowedFiles()
    {
        var xemId = Guid.NewGuid();
        var policy = new FakeTruyCapPolicy(
            nameof(LoaiDoiTuongTepTin.nhat_ky_hoat_dong),
            (_, cacTep) => cacTep.Where(t => t.MucDich == "check_in").Select(t => t.Id).ToHashSet());
        var svc = Service(policy);
        var duoc = await UploadVaConfirmAsync(svc, _userId, "check_in", "image/jpeg", Jpeg);
        var khong = await UploadVaConfirmAsync(svc, _userId, "progress", "image/jpeg", Jpeg);
        await Lien(duoc, LoaiDoiTuongTepTin.nhat_ky_hoat_dong);
        await Lien(khong, LoaiDoiTuongTepTin.nhat_ky_hoat_dong);

        var urls = await svc.BLLayUrlsAsync(xemId, [duoc, khong, Guid.NewGuid()], default);

        Assert.Equal([duoc], urls.Keys.ToArray());
        Assert.Equal(1, policy.SoLanGoi);
    }

    [Fact]
    public async Task LayUrls_OwnerSeesOwnFilesWithoutAskingPolicy()
    {
        var policy = new FakeTruyCapPolicy(nameof(LoaiDoiTuongTepTin.nhat_ky_hoat_dong), (_, _) => new HashSet<Guid>());
        var svc = Service(policy);
        var id = await UploadVaConfirmAsync(svc, _userId, "check_in", "image/jpeg", Jpeg);
        await Lien(id, LoaiDoiTuongTepTin.nhat_ky_hoat_dong);

        var urls = await svc.BLLayUrlsAsync(_userId, [id], default);

        Assert.Single(urls);
        Assert.Equal(0, policy.SoLanGoi);
    }

    private async Task Lien(Guid id, LoaiDoiTuongTepTin loai)
    {
        var e = (await _repo.DLGetByIdAsync(id, default))!;
        e.loai_doi_tuong = loai;
        e.doi_tuong_id = Guid.NewGuid();
        e.trang_thai = TrangThaiTepTin.da_lien_ket;
        await _repo.DLUpdateAsync(e, default);
    }
}

public class FilePolicyTests
{
    [Theory]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 }, true)]
    [InlineData("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 }, true)]
    [InlineData("image/jpeg", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 }, false)]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8 }, false)]
    public void KhopMagicBytes_Image(string mime, byte[] head, bool expected) =>
        Assert.Equal(expected, FilePolicy.KhopMagicBytes(mime, head));

    [Fact]
    public void KhopMagicBytes_Webp_Heic_Mp4_Mov_Pdf()
    {
        Assert.True(FilePolicy.KhopMagicBytes("image/webp", MauByte.Riff("WEBP")));
        Assert.False(FilePolicy.KhopMagicBytes("image/webp", MauByte.Riff("WAVE")));
        Assert.True(FilePolicy.KhopMagicBytes("image/heic", MauByte.Ftyp("heic")));
        Assert.True(FilePolicy.KhopMagicBytes("image/heic", MauByte.Ftyp("mif1")));
        Assert.False(FilePolicy.KhopMagicBytes("image/heic", MauByte.Ftyp("isom")));
        Assert.True(FilePolicy.KhopMagicBytes("video/mp4", MauByte.Ftyp("isom")));
        Assert.True(FilePolicy.KhopMagicBytes("video/quicktime", MauByte.Ftyp("qt  ")));
        Assert.True(FilePolicy.KhopMagicBytes("application/pdf", MauByte.Pdf));
        Assert.False(FilePolicy.KhopMagicBytes("application/pdf", System.Text.Encoding.ASCII.GetBytes("<html> tai lieu khac")));
    }

    [Fact]
    public void ChuanHoaMime_StripsParametersAndCase()
    {
        Assert.Equal("image/jpeg", FilePolicy.ChuanHoaMime(" Image/JPEG ; q=1"));
        Assert.Null(FilePolicy.ChuanHoaMime("  "));
    }
}
