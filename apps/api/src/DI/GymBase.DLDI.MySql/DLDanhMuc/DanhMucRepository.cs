using System.Linq.Expressions;
using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.Entities.DI;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLDI.MySql.DLDanhMuc;

/// <summary>Truy cập bảng <c>danh_muc</c>.</summary>
public sealed class DanhMucRepository(AppDbContext db) : BaseRepository<danh_muc>(db), IDanhMucRepository
{
    /// <summary>Tìm kiếm theo tên hoặc mã, dùng khi cần phân trang ở màn quản trị danh mục sau này.</summary>
    protected override IReadOnlyList<Expression<Func<danh_muc, string?>>> SearchColumns => [e => e.ten, e => e.ma];

    public async Task<IReadOnlyList<danh_muc>> DLFindDangDungAsync(
        IReadOnlyCollection<string>? maDinhDanhs, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking().Where(e => e.tinh_trang);
        if (maDinhDanhs is { Count: > 0 })
        {
            query = query.Where(e => maDinhDanhs.Contains(e.ma_dinh_danh));
        }

        // TODO(DANH-MUC): bảng chưa có cột thứ tự; khi cần kéo-thả thứ tự hiển thị thì thêm cột và sắp theo đó.
        return await query
            .OrderBy(e => e.ma_dinh_danh)
            .ThenBy(e => e.ten)
            .ToListAsync(cancellationToken);
    }
}
