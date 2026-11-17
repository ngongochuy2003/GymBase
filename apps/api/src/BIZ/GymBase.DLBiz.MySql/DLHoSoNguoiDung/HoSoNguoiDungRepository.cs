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

    public Task DLUpsertAsync(
        ho_so_nguoi_dung hoSo, chi_so_co_the? chiSoMoi, CancellationToken cancellationToken)
    {
        // DbContext bật EnableRetryOnFailure (Pomelo), nên EF không cho mở transaction thủ công: phải chạy toàn bộ đơn vị
        // công việc trong execution strategy để khi thử lại thì cả transaction được chạy lại từ đầu.
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            // Lần thử trước có thể đã gắn hồ sơ và chỉ số vào ChangeTracker rồi mới lỗi; bỏ đi để thêm lại cho sạch.
            db.ChangeTracker.Clear();

            // Hồ sơ và chỉ số cơ thể là hai bảng nhưng một lần bấm "Lưu thiết lập", nên ghi cùng transaction.
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            // AsTracking: DbContext mặc định NoTracking, mà SetValues bên dưới chỉ ghi được khi thực thể đang được theo dõi.
            var daCo = await db.Set<ho_so_nguoi_dung>()
                .AsTracking()
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
        });
    }
}
