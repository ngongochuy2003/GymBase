using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.Entities.Biz;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql.DLChiSoCoThe;

/// <summary>Truy cập bảng <c>chi_so_co_the</c>.</summary>
public sealed class ChiSoCoTheRepository(AppDbContext db) : IChiSoCoTheRepository
{
    public Task<chi_so_co_the?> DLFindMoiNhatAsync(Guid hocVienId, CancellationToken cancellationToken) =>
        db.Set<chi_so_co_the>()
            .AsNoTracking()
            .Where(c => c.hoc_vien_id == hocVienId)
            .OrderByDescending(c => c.ngay_do)
            .ThenByDescending(c => c.ngay_tao)
            .FirstOrDefaultAsync(cancellationToken);
}
