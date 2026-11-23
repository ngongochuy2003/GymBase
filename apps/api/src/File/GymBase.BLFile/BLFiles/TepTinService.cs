using GymBase.Base.Exceptions;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLFiles;

public sealed class TepTinService(
    IStorageResolver storageResolver,
    ITepTinRepository tepTins,
    IEnumerable<IFileTruyCapPolicy> policies,
    IOptions<StorageOptions> options,
    TimeProvider timeProvider,
    ILogger<TepTinService> logger) : ITepTinService
{
    private static readonly TimeSpan GioVietNam = TimeSpan.FromHours(7);
    private readonly StorageOptions _options = options.Value;

    public async Task<XinUploadResultDto> BLXinUploadAsync(
        Guid nguoiDungId, XinUploadRequestDto request, CancellationToken ct = default)
    {
        if (!FilePolicy.TryGetRule(request.MucDich, out var rule))
        {
            throw new ValidationFailedException(
                "muc_dich",
                $"Mục đích upload '{request.MucDich}' không hợp lệ. Cho phép: {string.Join(", ", FilePolicy.CacMucDich)}.",
                FileErrorCodes.PurposeInvalid);
        }

        var mime = FilePolicy.ChuanHoaMime(request.LoaiMime);
        var loai = FilePolicy.PhanLoai(mime);
        if (mime == null || loai is not { } loaiFile || !rule.Loai.Contains(loaiFile))
        {
            throw new ValidationFailedException(
                "loai_mime", $"Loại tệp '{request.LoaiMime}' không được phép cho mục đích '{rule.MucDich}'.", FileErrorCodes.TypeNotAllowed);
        }

        var toiDa = FilePolicy.DungLuongToiDa(loaiFile, _options);
        if (request.KichThuocByte <= 0 || request.KichThuocByte > toiDa)
        {
            throw new ValidationFailedException(
                "kich_thuoc_byte",
                request.KichThuocByte <= 0
                    ? "Tệp tải lên rỗng."
                    : $"Dung lượng tệp ({request.KichThuocByte} byte) vượt quá giới hạn ({toiDa} byte).",
                FileErrorCodes.TooLarge);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        await KiemHanMucAsync(nguoiDungId, loaiFile, request.KichThuocByte, now, ct);

        var provider = storageResolver.GetDefault();
        if (!provider.SupportsPresign)
        {
            throw new InvalidOperationException(
                $"Storage '{provider.ProviderName}' không ký được presigned URL. Đặt Storage:DefaultProvider là MINIO hoặc S3.");
        }

        var id = Guid.CreateVersion7();
        var entity = new tep_tin
        {
            Id = id,
            nguoi_tai_len_id = nguoiDungId,
            ten_tep_goc = TenTep(request.TenTep),
            muc_dich = rule.MucDich,
            s3_bucket = TenBucket(provider),
            s3_key = $"{rule.MucDich}/{now:yyyy}/{now:MM}/{id}{FilePolicy.DuoiTep(mime)}",
            loai_mime = mime,
            kich_thuoc_byte = request.KichThuocByte,
            che_do_truy_cap = rule.CheDoMacDinh,
            trang_thai = TrangThaiTepTin.cho_tai_len,
            ngay_tao = now,
        };
        await tepTins.DLInsertAsync(entity, ct);

        var expiry = TimeSpan.FromMinutes(_options.PutExpiryMinutes);
        var form = provider.CreatePresignedUpload(FilePolicy.KeyTam(id), mime, toiDa, expiry);
        return new XinUploadResultDto(id, new UploadTargetDto(form.Url, "POST", form.Fields, "file", now.Add(expiry)));
    }

    public async Task<XacNhanUploadResultDto> BLXacNhanUploadAsync(Guid nguoiDungId, Guid fileId, CancellationToken ct = default)
    {
        var entity = await tepTins.DLGetByIdAsync(fileId, ct);
        if (entity == null || entity.nguoi_tai_len_id != nguoiDungId)
        {
            throw new NotFoundException("Không tìm thấy tệp tin.");
        }

        if (entity.trang_thai is TrangThaiTepTin.da_tai_len or TrangThaiTepTin.da_lien_ket)
        {
            return ToResult(entity);
        }

        if (entity.trang_thai != TrangThaiTepTin.cho_tai_len)
        {
            throw new BusinessRuleException("Tệp tin không còn ở trạng thái chờ tải lên.", FileErrorCodes.InvalidState);
        }

        var storage = storageResolver.Resolve(entity.s3_bucket);
        var keyTam = FilePolicy.KeyTam(entity.Id);
        var info = await storage.GetInfoAsync(keyTam, ct);
        if (info == null)
        {
            throw new ValidationFailedException(
                "file_id", "Tệp chưa được tải lên kho lưu trữ.", FileErrorCodes.NotUploaded);
        }

        // Kích thước thật lấy từ storage; policy lúc ký đã giới hạn, đây là lớp kiểm thứ hai và ghi lại số liệu đúng cho hạn mức.
        var toiDa = FilePolicy.DungLuongToiDa(FilePolicy.PhanLoai(entity.loai_mime) ?? LoaiFile.TaiLieu, _options);
        var khop = info.Length > 0 && info.Length <= toiDa
                   && string.Equals(FilePolicy.ChuanHoaMime(info.ContentType), entity.loai_mime, StringComparison.Ordinal);
        if (khop)
        {
            var head = await storage.ReadHeadAsync(keyTam, FilePolicy.SoByteKiemMagic, ct);
            khop = FilePolicy.KhopMagicBytes(entity.loai_mime, head);
        }

        if (!khop)
        {
            logger.LogWarning(
                "Tệp {FileId} không khớp khai báo: khai {Mime}, thực tế {ActualMime} {ActualSize} byte (tối đa {Max}).",
                entity.Id, entity.loai_mime, info.ContentType, info.Length, toiDa);
            entity.trang_thai = TrangThaiTepTin.loi;
            await tepTins.DLUpdateAsync(entity, ct);
            await storage.DeleteAsync(keyTam, ct);
            throw new ValidationFailedException(
                "file_id", "Nội dung tệp không khớp với khai báo khi xin tải lên.", FileErrorCodes.ContentMismatch);
        }

        // Copy trước, ghi DB sau, xóa key tạm cuối: lỗi giữa chừng thì gọi lại confirm được; key tạm sót lại để job dọn / lifecycle xóa.
        await storage.CopyAsync(keyTam, entity.s3_key, ct);
        entity.kich_thuoc_byte = info.Length;
        entity.trang_thai = TrangThaiTepTin.da_tai_len;
        entity.ngay_xac_nhan = timeProvider.GetUtcNow().UtcDateTime;
        await tepTins.DLUpdateAsync(entity, ct);

        try
        {
            await storage.DeleteAsync(keyTam, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không xóa được key tạm {Key} của tệp {FileId}; để job dọn xử lý.", keyTam, entity.Id);
        }

        // TODO(FILE): ghi dòng outbox xử lý nền (bỏ EXIF, blurhash, ảnh xem trước video) cùng transaction với dòng trên.
        return ToResult(entity);
    }

    public async Task<FileUrlDto> BLLayUrlAsync(Guid nguoiXemId, Guid fileId, CancellationToken ct = default)
    {
        var urls = await BLLayUrlsAsync(nguoiXemId, [fileId], ct);
        return urls.TryGetValue(fileId, out var url) ? url : throw new NotFoundException("Không tìm thấy tệp tin.");
    }

    public async Task<IReadOnlyDictionary<Guid, FileUrlDto>> BLLayUrlsAsync(
        Guid nguoiXemId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default)
    {
        var ketQua = new Dictionary<Guid, FileUrlDto>();
        if (fileIds.Count == 0)
        {
            return ketQua;
        }

        var caTep = (await tepTins.DLFindByIdsAsync(fileIds.Distinct().ToList(), ct))
            .Where(t => t.trang_thai is TrangThaiTepTin.da_tai_len or TrangThaiTepTin.da_lien_ket)
            .ToList();
        var duocXem = await XacDinhDuocXemAsync(nguoiXemId, caTep, ct);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var tep in caTep.Where(t => duocXem.Contains(t.Id)))
        {
            var congKhai = tep.che_do_truy_cap == CheDoTruyCap.cong_khai;
            var expiry = TimeSpan.FromMinutes(congKhai ? _options.GetPublicExpiryMinutes : _options.GetPrivateExpiryMinutes);
            var url = storageResolver.Resolve(tep.s3_bucket).CreatePresignedGetUrl(tep.s3_key, expiry);
            ketQua[tep.Id] = new FileUrlDto(tep.Id, url, now.Add(expiry), tep.loai_mime);
        }

        return ketQua;
    }

    /// <summary>
    /// Chủ file luôn xem được. File đã gắn vào đối tượng có policy thì policy quyết định (theo lô); chưa có policy thì file công khai
    /// xem được, file riêng tư không. File chưa gắn chỉ chủ xem được.
    /// </summary>
    private async Task<HashSet<Guid>> XacDinhDuocXemAsync(Guid nguoiXemId, List<tep_tin> caTep, CancellationToken ct)
    {
        var duocXem = caTep.Where(t => t.nguoi_tai_len_id == nguoiXemId).Select(t => t.Id).ToHashSet();
        var nguoiKhac = caTep.Where(t => !duocXem.Contains(t.Id) && t.trang_thai == TrangThaiTepTin.da_lien_ket).ToList();

        foreach (var nhom in nguoiKhac.GroupBy(t => t.loai_doi_tuong?.ToString()))
        {
            var policy = nhom.Key is { } loai ? policies.FirstOrDefault(p => p.LoaiDoiTuong == loai) : null;
            if (policy != null)
            {
                var cacTep = nhom.Select(t => new TepTinTruyCap(t.Id, t.nguoi_tai_len_id, t.doi_tuong_id, t.muc_dich)).ToList();
                duocXem.UnionWith(await policy.BLKiemTruyCapAsync(nguoiXemId, cacTep, ct));
            }
            else
            {
                // TODO(FILE): thêm IFileTruyCapPolicy cho check_in, ảnh cơ thể (chủ chọn chia sẻ PT) khi nối API lộ trình.
                duocXem.UnionWith(nhom.Where(t => t.che_do_truy_cap == CheDoTruyCap.cong_khai).Select(t => t.Id));
            }
        }

        return duocXem;
    }

    public async Task BLGanAsync(
        Guid nguoiDungId,
        IReadOnlyCollection<Guid> fileIds,
        string loaiDoiTuong,
        Guid doiTuongId,
        IReadOnlyCollection<string> mucDichChoPhep,
        CancellationToken ct = default)
    {
        if (fileIds.Count == 0)
        {
            return;
        }

        if (!Enum.TryParse<LoaiDoiTuongTepTin>(loaiDoiTuong, ignoreCase: false, out var loai))
        {
            throw new ArgumentException($"Loại đối tượng '{loaiDoiTuong}' không hợp lệ.", nameof(loaiDoiTuong));
        }

        var thuTu = fileIds.ToList();
        if (thuTu.Distinct().Count() != thuTu.Count)
        {
            throw new ValidationFailedException("file_ids", "Danh sách tệp đính kèm bị trùng.");
        }

        var caTep = (await tepTins.DLFindByIdsAsync(thuTu, ct)).ToDictionary(t => t.Id);
        if (caTep.Count != thuTu.Count || caTep.Values.Any(t => t.nguoi_tai_len_id != nguoiDungId))
        {
            throw new ValidationFailedException("file_ids", "Tệp đính kèm không tồn tại hoặc không phải của bạn.");
        }

        foreach (var tep in caTep.Values)
        {
            if (tep.trang_thai != TrangThaiTepTin.da_tai_len)
            {
                throw new ValidationFailedException(
                    "file_ids", "Tệp đính kèm chưa tải lên xong hoặc đã được dùng ở nơi khác.", FileErrorCodes.InvalidState);
            }

            if (!mucDichChoPhep.Contains(tep.muc_dich ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationFailedException(
                    "file_ids", "Tệp đính kèm không dùng được cho mục này.", FileErrorCodes.PurposeInvalid);
            }
        }

        for (var i = 0; i < thuTu.Count; i++)
        {
            var tep = caTep[thuTu[i]];
            tep.loai_doi_tuong = loai;
            tep.doi_tuong_id = doiTuongId;
            tep.thu_tu_hien_thi = i + 1;
            tep.trang_thai = TrangThaiTepTin.da_lien_ket;
            await tepTins.DLUpdateAsync(tep, ct);
        }
    }

    public async Task BLXoaAsync(Guid nguoiDungId, Guid fileId, CancellationToken ct = default)
    {
        var entity = await tepTins.DLGetByIdAsync(fileId, ct);
        if (entity == null || entity.nguoi_tai_len_id != nguoiDungId)
        {
            throw new NotFoundException("Không tìm thấy tệp tin.");
        }

        if (entity.trang_thai == TrangThaiTepTin.da_lien_ket)
        {
            throw new BusinessRuleException("Tệp tin đang được sử dụng nên không xóa được.", FileErrorCodes.InvalidState);
        }

        var storage = storageResolver.Resolve(entity.s3_bucket);
        await storage.DeleteAsync(FilePolicy.KeyTam(entity.Id), ct);
        await storage.DeleteAsync(entity.s3_key, ct);
        await tepTins.DLDeleteAsync(entity.Id, ct);
    }

    private async Task KiemHanMucAsync(Guid nguoiDungId, LoaiFile loai, long kichThuoc, DateTime now, CancellationToken ct)
    {
        var dauNgayUtc = (now + GioVietNam).Date - GioVietNam;
        var homNay = await tepTins.DLFindByUploaderSinceAsync(nguoiDungId, dauNgayUtc, ct);

        if (homNay.Count(t => t.trang_thai == TrangThaiTepTin.cho_tai_len) >= _options.MaxPendingUploads)
        {
            throw Vuot("Bạn đang có quá nhiều tệp chờ tải lên. Hãy thử lại sau ít phút.");
        }

        var soLuong = homNay.Count(t => FilePolicy.PhanLoai(t.loai_mime) == loai);
        var toiDaSoLuong = loai switch
        {
            LoaiFile.Anh => _options.MaxImagesPerDay,
            LoaiFile.Video => _options.MaxVideosPerDay,
            _ => _options.MaxDocumentsPerDay,
        };
        if (soLuong >= toiDaSoLuong)
        {
            throw Vuot("Bạn đã đạt số lượng tệp tối đa được tải lên trong ngày.");
        }

        if (homNay.Sum(t => t.kich_thuoc_byte) + kichThuoc > _options.MaxBytesPerDay)
        {
            throw Vuot("Bạn đã đạt dung lượng tối đa được tải lên trong ngày.");
        }

        static BusinessRuleException Vuot(string detail) => new(detail, FileErrorCodes.QuotaExceeded);
    }

    private string TenBucket(IFileStorage provider) =>
        string.Equals(provider.ProviderName, "LOCAL", StringComparison.OrdinalIgnoreCase)
            ? "LOCAL"
            : string.IsNullOrWhiteSpace(_options.BucketName) ? provider.ProviderName : _options.BucketName;

    private static string? TenTep(string? tenTep)
    {
        var ten = string.IsNullOrWhiteSpace(tenTep) ? null : Path.GetFileName(tenTep.Trim());
        return ten is { Length: > 200 } ? ten[..200] : ten;
    }

    private static XacNhanUploadResultDto ToResult(tep_tin tep) =>
        new(tep.Id, tep.loai_mime, tep.kich_thuoc_byte, tep.muc_dich ?? string.Empty, tep.trang_thai.ToString());
}
