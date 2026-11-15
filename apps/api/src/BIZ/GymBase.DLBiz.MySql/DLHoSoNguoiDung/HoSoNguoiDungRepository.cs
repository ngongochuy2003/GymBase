using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql.DLHoSoNguoiDung;

/// <summary>Truy cập bảng <c>ho_so_nguoi_dung</c> (khóa chính <c>tai_khoan_id</c>).</summary>
public sealed class HoSoNguoiDungRepository(AppDbContext db) : IHoSoNguoiDungRepository
{
    public Task<ho_so_nguoi_dung?> DLFindByTaiKhoanIdAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        db.Set<ho_so_nguoi_dung>()
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.tai_khoan_id == taiKhoanId, cancellationToken);

    public async Task DLUpsertAsync(
        ho_so_nguoi_dung hoSo, chi_so_co_the? chiSoMoi, CancellationToken cancellationToken)
    {
        // Hồ sơ và chỉ số cơ thể là hai bảng nhưng một lần bấm "Lưu thiết lập", nên ghi cùng transaction.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var daCo = await db.Set<ho_so_nguoi_dung>()
            .FirstOrDefaultAsync(h => h.tai_khoan_id == hoSo.tai_khoan_id, cancellationToken);
        if (daCo is null)
        {
            db.Add(hoSo);
        }
        else
        {
            db.Entry(daCo).CurrentValues.SetValues(hoSo);
        }

        if (chiSoMoi is not null)
        {
            db.Add(chiSoMoi);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
