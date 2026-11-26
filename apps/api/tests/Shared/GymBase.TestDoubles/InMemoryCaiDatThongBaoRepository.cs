using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;

namespace GymBase.TestDoubles;

public sealed class InMemoryCaiDatThongBaoRepository(InMemoryStore<cai_dat_thong_bao> store)
    : InMemoryBaseRepository<cai_dat_thong_bao>(store), ICaiDatThongBaoRepository
{
    public Task<IReadOnlyList<cai_dat_thong_bao>> DLFindByTaiKhoanAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<cai_dat_thong_bao>>(Store.All().Where(t => t.tai_khoan_id == taiKhoanId).ToList());
}
