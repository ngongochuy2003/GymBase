using GymBase.Base.Paging;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.TestDoubles;

public sealed class InMemoryTepTinRepository : InMemoryBaseRepository<tep_tin>, ITepTinRepository
{
    public InMemoryTepTinRepository(InMemoryStore<tep_tin> store) : base(store)
    {
    }

    protected override IEnumerable<string?> GetSearchValues(tep_tin entity) =>
        [entity.ten_tep_goc, entity.s3_key, entity.muc_dich];

    protected override QueryColumns<tep_tin> AllowedColumns => TepTinQueryColumns.Allowed;

    public Task<IReadOnlyList<tep_tin>> DLGetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default)
    {
        var list = Store.All()
            .Where(t => (t.s3_bucket == provider || t.s3_bucket == "LOCAL") && t.trang_thai == TrangThaiTepTin.da_tai_len)
            .OrderBy(t => t.ngay_tao)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<tep_tin>>(list);
    }

    public Task<IReadOnlyList<tep_tin>> DLFindLinkedAsync(
        LoaiDoiTuongTepTin loaiDoiTuong, Guid doiTuongId, string mucDich, CancellationToken ct = default)
    {
        var list = Store.All()
            .Where(t => t.loai_doi_tuong == loaiDoiTuong && t.doi_tuong_id == doiTuongId && t.muc_dich == mucDich
                && t.trang_thai == TrangThaiTepTin.da_lien_ket)
            .OrderByDescending(t => t.ngay_tao)
            .ToList();

        return Task.FromResult<IReadOnlyList<tep_tin>>(list);
    }
}
