using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql;

/// <summary>Truy cập bảng <c>bai_viet</c> và dữ liệu đính kèm trên bảng tin bằng EF Core (MySQL).</summary>
public sealed class BaiVietRepository(AppDbContext db) : BaseRepository<bai_viet>(db), IBaiVietRepository
{
    private const string ExternalBucket = "external";

    protected override IReadOnlyList<Expression<Func<bai_viet, string?>>> SearchColumns { get; } =
        [e => e.noi_dung];

    protected override QueryColumns<bai_viet> AllowedColumns => BaiVietQueryColumns.Allowed;

    public async Task<PagingResult<bai_viet>> DLGetFeedPagingAsync(
        string? tab,
        Guid? currentUserId,
        PagingRequest request,
        CancellationToken cancellationToken)
    {
        var normalized = request.Normalize();

        IQueryable<bai_viet> query = Set.AsNoTracking()
            .Where(p => p.trang_thai == TrangThaiBaiViet.da_dang);

        if (currentUserId is { } uid)
        {
            query = query.Where(p =>
                p.che_do_hien_thi == CheDoHienThi.cong_khai
                || p.tac_gia_id == uid
                || (p.che_do_hien_thi == CheDoHienThi.nguoi_theo_doi
                    && Db.Set<theo_doi>().Any(f => f.nguoi_theo_doi_id == uid && f.nguoi_duoc_theo_doi_id == p.tac_gia_id)));
        }
        else
        {
            query = query.Where(p => p.che_do_hien_thi != CheDoHienThi.rieng_tu);
        }

        if (normalized.search is { Length: > 0 } search)
        {
            query = query.Where(p => p.noi_dung != null && p.noi_dung.Contains(search));
        }

        if (normalized.filter is { Length: > 0 } filter)
        {
            query = PagingQuery.ApplyFilter(query, filter, AllowedColumns);
        }

        var normalizedTab = tab?.Trim().ToLowerInvariant();
        if (normalizedTab == "workout")
        {
            query = query.Where(p =>
                Db.Set<tep_tin>().Any(t =>
                    t.loai_doi_tuong == LoaiDoiTuongTepTin.bai_viet
                    && t.doi_tuong_id == p.Id
                    && t.trang_thai != TrangThaiTepTin.da_xoa
                    && t.muc_dich == "workout")
                || (p.noi_dung != null && (
                    p.noi_dung.Contains("#workout")
                    || p.noi_dung.Contains("#tapluyen")
                    || p.noi_dung.Contains("Push")
                    || p.noi_dung.Contains("Pull")
                    || p.noi_dung.Contains("Squat")
                    || p.noi_dung.Contains("tập"))));
        }
        else if (normalizedTab is "knowledge" or "meal-prep" or "tips" or "progress")
        {
            query = query.Where(p =>
                Db.Set<tep_tin>().Any(t =>
                    t.loai_doi_tuong == LoaiDoiTuongTepTin.bai_viet
                    && t.doi_tuong_id == p.Id
                    && t.trang_thai != TrangThaiTepTin.da_xoa
                    && (t.muc_dich == "knowledge"
                        || t.muc_dich == "meal-prep"
                        || t.muc_dich == "tips"
                        || t.muc_dich == "progress"))
                || (p.noi_dung != null && (
                    p.noi_dung.Contains("#kienthuc")
                    || p.noi_dung.Contains("#mealprep")
                    || p.noi_dung.Contains("#meohay")
                    || p.noi_dung.Contains("Meal prep")
                    || p.noi_dung.Contains("siết mỡ")
                    || p.noi_dung.Contains("protein")
                    || p.noi_dung.Contains("Mẹo"))));
        }

        IQueryable<bai_viet> sorted;
        if (normalizedTab == "trending")
        {
            sorted = query
                .OrderByDescending(p => Db.Set<tuong_tac_bai_viet>()
                    .Count(t => t.bai_viet_id == p.Id && t.ngay_xoa == null))
                .ThenByDescending(p => p.ngay_dang ?? p.ngay_tao)
                .ThenByDescending(p => p.Id);
        }
        else if (normalized.sort is { Length: > 0 } sort)
        {
            sorted = PagingQuery.ApplySort(query, sort, AllowedColumns);
        }
        else
        {
            sorted = query
                .OrderByDescending(p => p.ngay_dang ?? p.ngay_tao)
                .ThenByDescending(p => p.Id);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await sorted.Skip(normalized.Skip).Take(normalized.page_size).ToListAsync(cancellationToken);

        return new PagingResult<bai_viet>(rows, normalized.page, normalized.page_size, total);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<tep_tin>>> DLGetMediaByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken)
    {
        if (postIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<tep_tin>>();
        }

        var files = await Db.Set<tep_tin>()
            .AsNoTracking()
            .Where(t =>
                t.loai_doi_tuong == LoaiDoiTuongTepTin.bai_viet
                && t.doi_tuong_id != null
                && postIds.Contains(t.doi_tuong_id.Value)
                && t.trang_thai != TrangThaiTepTin.da_xoa)
            .OrderBy(t => t.thu_tu_hien_thi ?? 0)
            .ThenBy(t => t.ngay_tao)
            .ToListAsync(cancellationToken);

        return files
            .GroupBy(t => t.doi_tuong_id!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<tep_tin>)g.ToList());
    }

    public async Task DLAttachMediaUrlsAsync(
        bai_viet post,
        IReadOnlyList<string> mediaUrls,
        string? chuDe,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (mediaUrls.Count == 0)
        {
            return;
        }

        var fileSet = Db.Set<tep_tin>();
        var entities = new List<tep_tin>(mediaUrls.Count);
        var purpose = string.IsNullOrWhiteSpace(chuDe) ? "workout" : chuDe.Trim();
        if (purpose.Length > 32)
        {
            purpose = purpose[..32];
        }

        for (var i = 0; i < mediaUrls.Count; i++)
        {
            var rawUrl = mediaUrls[i]?.Trim();
            if (string.IsNullOrEmpty(rawUrl))
            {
                continue;
            }

            var fileId = Guid.CreateVersion7();
            var uniqueKey = $"{rawUrl}#tep={fileId:N}";
            if (uniqueKey.Length > 512)
            {
                uniqueKey = uniqueKey[..512];
            }

            var file = new tep_tin
            {
                Id = fileId,
                nguoi_tai_len_id = post.tac_gia_id,
                loai_doi_tuong = LoaiDoiTuongTepTin.bai_viet,
                doi_tuong_id = post.Id,
                muc_dich = purpose,
                s3_bucket = ExternalBucket,
                s3_key = uniqueKey,
                ten_tep_goc = $"post-media-{i + 1}.jpg",
                loai_mime = "image/jpeg",
                kich_thuoc_byte = 0,
                che_do_truy_cap = post.che_do_hien_thi == CheDoHienThi.rieng_tu
                    ? CheDoTruyCap.rieng_tu
                    : CheDoTruyCap.cong_khai,
                trang_thai = TrangThaiTepTin.da_lien_ket,
                thu_tu_hien_thi = i + 1,
                ngay_tao = utcNow,
                ngay_xac_nhan = utcNow,
            };
            entities.Add(file);
            fileSet.Add(file);
        }

        if (entities.Count == 0)
        {
            return;
        }

        try
        {
            await Db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            foreach (var entity in entities)
            {
                Db.Entry(entity).State = EntityState.Detached;
            }
        }
    }

    public async Task<IReadOnlyDictionary<Guid, ho_so_nguoi_dung>> DLGetProfilesByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0)
        {
            return new Dictionary<Guid, ho_so_nguoi_dung>();
        }

        var profiles = await Db.Set<ho_so_nguoi_dung>()
            .AsNoTracking()
            .Where(h => accountIds.Contains(h.tai_khoan_id))
            .ToListAsync(cancellationToken);

        return profiles.ToDictionary(h => h.tai_khoan_id);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> DLGetAvatarUrlsByFileIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken)
    {
        if (fileIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var files = await Db.Set<tep_tin>()
            .AsNoTracking()
            .Where(t => fileIds.Contains(t.Id) && t.trang_thai != TrangThaiTepTin.da_xoa)
            .ToListAsync(cancellationToken);

        return files.ToDictionary(t => t.Id, ResolveFileUrl);
    }

    private static string ResolveFileUrl(tep_tin file)
    {
        var key = file.s3_key;
        var hashIndex = key.IndexOf("#tep=", StringComparison.Ordinal);
        if (hashIndex > 0)
        {
            key = key[..hashIndex];
        }

        if (key.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return key;
        }

        return $"/api/files/{file.Id}/content";
    }
}
