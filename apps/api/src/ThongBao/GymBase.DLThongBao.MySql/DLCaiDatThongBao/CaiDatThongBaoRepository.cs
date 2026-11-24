using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLThongBao.MySql;

public sealed class CaiDatThongBaoRepository(AppDbContext db) : BaseRepository<cai_dat_thong_bao>(db), ICaiDatThongBaoRepository
{
    public async Task<IReadOnlyList<cai_dat_thong_bao>> DLFindByTaiKhoanAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        await Set.AsNoTracking().Where(t => t.tai_khoan_id == taiKhoanId).ToListAsync(ct);
}
