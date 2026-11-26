using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLThongBao.MySql;

public sealed class ThietBiRepository(AppDbContext db) : BaseRepository<thiet_bi>(db), IThietBiRepository
{
    public Task<thiet_bi?> DLFindByMaThietBiAsync(Guid taiKhoanId, string maThietBi, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(t => t.tai_khoan_id == taiKhoanId && t.ma_thiet_bi == maThietBi, ct);

    public async Task<IReadOnlyList<thiet_bi>> DLFindActiveWithTokenAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(t => t.tai_khoan_id == taiKhoanId && t.dang_hoat_dong && t.push_token != null)
            .ToListAsync(ct);

    public async Task DLClearPushTokenAsync(Guid thietBiId, CancellationToken ct = default) =>
        await Set
            .Where(t => t.Id == thietBiId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.push_token, (string?)null).SetProperty(t => t.dang_hoat_dong, false), ct);

    public async Task DLReleasePushTokenAsync(string pushToken, Guid tru, CancellationToken ct = default) =>
        await Set
            .Where(t => t.push_token == pushToken && t.Id != tru)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.push_token, (string?)null).SetProperty(t => t.dang_hoat_dong, false), ct);
}
