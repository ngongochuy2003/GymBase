using GymBase.Base.Paging;
using GymBase.DLFile.Contract;
using GymBase.Entities.File;

namespace GymBase.TestDoubles;

public sealed class InMemoryTepTinRepository : InMemoryBaseRepository<tep_tin>, ITepTinRepository
{
    public InMemoryTepTinRepository(InMemoryStore<tep_tin> store) : base(store)
    {
    }

    protected override IEnumerable<string?> GetSearchValues(tep_tin entity) =>
        [entity.ten_tep_goc, entity.khoa_luu_tru, entity.muc_dich];

    protected override QueryColumns<tep_tin> AllowedColumns => TepTinQueryColumns.Allowed;

    public Task<IReadOnlyList<tep_tin>> GetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default)
    {
        var list = Store.All()
            .Where(t => !t.is_deleted && t.nha_cung_cap == provider && t.trang_thai == "READY")
            .OrderBy(t => t.ngay_tao)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<tep_tin>>(list);
    }
}
