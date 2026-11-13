using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLFile.Contract;
using GymBase.Entities.File;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLFile.MySql;

public sealed class TepTinRepository(AppDbContext db) : BaseRepository<tep_tin>(db), ITepTinRepository
{
    protected override IReadOnlyList<Expression<Func<tep_tin, string?>>> SearchColumns { get; } =
        [e => e.ten_tep_goc, e => e.khoa_luu_tru, e => e.muc_dich];

    protected override QueryColumns<tep_tin> AllowedColumns => TepTinQueryColumns.Allowed;

    public async Task<IReadOnlyList<tep_tin>> DLGetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default)
    {
        return await Set
            .Where(t => t.nha_cung_cap == provider && t.trang_thai == "READY")
            .OrderBy(t => t.ngay_tao)
            .Take(limit)
            .ToListAsync(ct);
    }
}
