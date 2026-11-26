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
        IReadOnlyCollection<string>? maDinhDanhs, string? maCapTren, CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking().Where(e => e.tinh_trang);
        if (maDinhDanhs is { Count: > 0 })
        {
            query = query.Where(e => maDinhDanhs.Contains(e.ma_dinh_danh));
        }

        query = maCapTren == null
            ? query.Where(e => e.danh_muc_cap_tren_id == null)
            : query.Where(e => Set.Any(cha =>
                cha.Id == e.danh_muc_cap_tren_id && cha.ma_dinh_danh == e.ma_dinh_danh && cha.ma == maCapTren));

        // TODO(DANH-MUC): bảng chưa có cột thứ tự; khi cần kéo-thả thứ tự hiển thị thì thêm cột và sắp theo đó.
        return await query
            .OrderBy(e => e.ma_dinh_danh)
            .ThenBy(e => e.ten)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> DLExistsByMaAsync(string maDinhDanh, string ma, CancellationToken cancellationToken) =>
        Set.AnyAsync(e => e.ma_dinh_danh == maDinhDanh && e.ma == ma && e.tinh_trang, cancellationToken);
}
