using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLThongBao.Contract;

public interface ICaiDatThongBaoRepository : IBaseRepository<cai_dat_thong_bao>
{
    Task<IReadOnlyList<cai_dat_thong_bao>> DLFindByTaiKhoanAsync(Guid taiKhoanId, CancellationToken ct = default);
}
