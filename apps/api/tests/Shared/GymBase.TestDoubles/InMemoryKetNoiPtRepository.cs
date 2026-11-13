using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.TestDoubles;

/// <summary>Repository kết nối PT giả cho test.</summary>
public sealed class InMemoryKetNoiPtRepository(InMemoryStore<ket_noi_pt> store)
    : InMemoryBaseRepository<ket_noi_pt>(store), IKetNoiPtRepository
{
    public Task<ket_noi_pt?> DLFindDangHoatDongAsync(Guid ptId, Guid hocVienId, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().FirstOrDefault(
            k => k.pt_id == ptId && k.hoc_vien_id == hocVienId && k.trang_thai == TrangThaiKetNoiPt.dang_hoat_dong));
}
