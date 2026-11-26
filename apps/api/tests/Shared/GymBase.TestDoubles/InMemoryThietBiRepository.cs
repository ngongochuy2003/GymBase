using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;

namespace GymBase.TestDoubles;

public sealed class InMemoryThietBiRepository(InMemoryStore<thiet_bi> store) : InMemoryBaseRepository<thiet_bi>(store), IThietBiRepository
{
    public Task<thiet_bi?> DLFindByMaThietBiAsync(Guid taiKhoanId, string maThietBi, CancellationToken ct = default) =>
        Task.FromResult(Store.All().FirstOrDefault(t => t.tai_khoan_id == taiKhoanId && t.ma_thiet_bi == maThietBi));

    public Task<IReadOnlyList<thiet_bi>> DLFindActiveWithTokenAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<thiet_bi>>(
            Store.All().Where(t => t.tai_khoan_id == taiKhoanId && t.dang_hoat_dong && t.push_token != null).ToList());

    public Task DLClearPushTokenAsync(Guid thietBiId, CancellationToken ct = default)
    {
        if (Store.Find(thietBiId) is { } t)
        {
            t.push_token = null;
            t.dang_hoat_dong = false;
            Store.TryReplace(t);
        }

        return Task.CompletedTask;
    }

    public Task DLReleasePushTokenAsync(string pushToken, Guid tru, CancellationToken ct = default)
    {
        foreach (var t in Store.All().Where(t => t.push_token == pushToken && t.Id != tru))
        {
            t.push_token = null;
            t.dang_hoat_dong = false;
            Store.TryReplace(t);
        }

        return Task.CompletedTask;
    }
}
