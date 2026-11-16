using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLFile.MySql;

public sealed class TepTinRepository(AppDbContext db) : BaseRepository<tep_tin>(db), ITepTinRepository
{
    protected override IReadOnlyList<Expression<Func<tep_tin, string?>>> SearchColumns { get; } =
        [e => e.ten_tep_goc, e => e.s3_key, e => e.muc_dich];

    protected override QueryColumns<tep_tin> AllowedColumns => TepTinQueryColumns.Allowed;

    public async Task<IReadOnlyList<tep_tin>> DLGetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default)
    {
        return await Set
            .Where(t => (t.s3_bucket == provider || t.s3_bucket == "LOCAL") && t.trang_thai == TrangThaiTepTin.da_tai_len)
            .OrderBy(t => t.ngay_tao)
            .Take(limit)
            .ToListAsync(ct);
    }
}
