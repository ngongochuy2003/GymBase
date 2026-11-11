using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql.DLHoSoNguoiDung;

/// <summary>Truy cập bảng <c>ho_so_nguoi_dung</c>.</summary>
public sealed class HoSoNguoiDungRepository(AppDbContext db) : IHoSoNguoiDungRepository
{
    public Task<ho_so_nguoi_dung?> DLFindByTaiKhoanIdAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        db.Set<ho_so_nguoi_dung>().FirstOrDefaultAsync(h => h.tai_khoan_id == taiKhoanId, cancellationToken);
}
