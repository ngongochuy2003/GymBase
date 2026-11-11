using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql.DLKetNoiPt;

/// <summary>Truy cập bảng <c>ket_noi_pt</c>.</summary>
public sealed class KetNoiPtRepository(AppDbContext db) : BaseRepository<ket_noi_pt>(db), IKetNoiPtRepository
{
    public Task<ket_noi_pt?> DLFindDangHoatDongAsync(Guid ptId, Guid hocVienId, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(
            k => k.pt_id == ptId && k.hoc_vien_id == hocVienId && k.trang_thai == TrangThaiKetNoiPt.dang_hoat_dong,
            cancellationToken);
}
