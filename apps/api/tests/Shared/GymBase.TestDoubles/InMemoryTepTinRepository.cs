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
}
