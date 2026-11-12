using GymBase.Base.Paging;
using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.TestDoubles;

/// <summary><see cref="IBaiVietRepository"/> trong bộ nhớ dùng cho unit/integration test.</summary>
public sealed class InMemoryBaiVietRepository(InMemoryStore<bai_viet> store)
    : InMemoryBaseRepository<bai_viet>(store), IBaiVietRepository
{
    private readonly List<tep_tin> _files = [];
    private readonly Dictionary<Guid, ho_so_nguoi_dung> _profiles = new();

    protected override IEnumerable<string?> GetSearchValues(bai_viet entity) => [entity.noi_dung];

    protected override QueryColumns<bai_viet> AllowedColumns => BaiVietQueryColumns.Allowed;

    public void SeedProfile(ho_so_nguoi_dung profile) => _profiles[profile.tai_khoan_id] = profile;

    public Task<PagingResult<bai_viet>> GetFeedPagingAsync(
        string? tab,
        Guid? currentUserId,
        PagingRequest request,
        CancellationToken cancellationToken)
    {
        var normalized = request.Normalize();
        var items = Store.All()
            .Where(p => p.trang_thai == TrangThaiBaiViet.da_dang)
            .Where(p => currentUserId.HasValue
                ? p.che_do_hien_thi != CheDoHienThi.rieng_tu || p.tac_gia_id == currentUserId.Value
                : p.che_do_hien_thi != CheDoHienThi.rieng_tu);

        if (normalized.search is { Length: > 0 } search)
        {
            items = items.Where(p => p.noi_dung != null && p.noi_dung.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var normalizedTab = tab?.Trim().ToLowerInvariant();
        if (normalizedTab == "workout")
        {
            items = items.Where(p =>
                _files.Any(f => f.doi_tuong_id == p.Id && f.muc_dich == "workout")
                || (p.noi_dung != null && (
                    p.noi_dung.Contains("Push", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("tập", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("#tapluyen", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("#workout", StringComparison.OrdinalIgnoreCase))));
        }
        else if (normalizedTab is "knowledge" or "meal-prep" or "tips" or "progress")
        {
            items = items.Where(p =>
                _files.Any(f => f.doi_tuong_id == p.Id && f.muc_dich is "knowledge" or "meal-prep" or "tips" or "progress")
                || (p.noi_dung != null && (
                    p.noi_dung.Contains("Meal prep", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("siết mỡ", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("#mealprep", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("#kienthuc", StringComparison.OrdinalIgnoreCase)
                    || p.noi_dung.Contains("#meohay", StringComparison.OrdinalIgnoreCase))));
        }

        var list = items
            .OrderByDescending(p => p.ngay_dang ?? p.ngay_tao)
            .ThenByDescending(p => p.Id)
            .ToList();

        var page = list.Skip(normalized.Skip).Take(normalized.page_size).ToList();
        return Task.FromResult(new PagingResult<bai_viet>(page, normalized.page, normalized.page_size, list.Count));
    }

    public Task<IReadOnlyDictionary<Guid, IReadOnlyList<tep_tin>>> GetMediaByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken)
    {
        var dict = _files
            .Where(f => f.loai_doi_tuong == LoaiDoiTuongTepTin.bai_viet && f.doi_tuong_id.HasValue && postIds.Contains(f.doi_tuong_id.Value))
            .OrderBy(f => f.thu_tu_hien_thi ?? 0)
            .GroupBy(f => f.doi_tuong_id!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<tep_tin>)g.ToList());

        return Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<tep_tin>>>(dict);
    }

    public Task AttachMediaUrlsAsync(
        bai_viet post,
        IReadOnlyList<string> mediaUrls,
        string? chuDe,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var purpose = string.IsNullOrWhiteSpace(chuDe) ? "workout" : chuDe.Trim();
        for (var i = 0; i < mediaUrls.Count; i++)
        {
            var raw = mediaUrls[i]?.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                continue;
            }

            var fileId = Guid.CreateVersion7();
            _files.Add(new tep_tin
            {
                Id = fileId,
                nguoi_tai_len_id = post.tac_gia_id,
                loai_doi_tuong = LoaiDoiTuongTepTin.bai_viet,
                doi_tuong_id = post.Id,
                muc_dich = purpose,
                s3_bucket = "external",
                s3_key = $"{raw}#tep={fileId:N}",
                ten_tep_goc = $"post-{i + 1}.jpg",
                loai_mime = "image/jpeg",
                kich_thuoc_byte = 0,
                che_do_truy_cap = CheDoTruyCap.cong_khai,
                trang_thai = TrangThaiTepTin.da_lien_ket,
                thu_tu_hien_thi = i + 1,
                ngay_tao = utcNow,
                ngay_xac_nhan = utcNow,
            });
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<Guid, ho_so_nguoi_dung>> GetProfilesByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        var result = accountIds
            .Where(_profiles.ContainsKey)
            .ToDictionary(id => id, id => _profiles[id]);
        return Task.FromResult<IReadOnlyDictionary<Guid, ho_so_nguoi_dung>>(result);
    }

    public Task<IReadOnlyDictionary<Guid, string>> GetAvatarUrlsByFileIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken)
    {
        var result = _files
            .Where(f => fileIds.Contains(f.Id))
            .ToDictionary(f => f.Id, f => f.s3_key.Split("#tep=")[0]);
        return Task.FromResult<IReadOnlyDictionary<Guid, string>>(result);
    }
}
