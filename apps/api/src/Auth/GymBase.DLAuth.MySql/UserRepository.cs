using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLAuth.Contract;
using GymBase.Entities.Auth;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLAuth.MySql;

/// <summary>Truy cập bảng <c>tai_khoan</c>.</summary>
public sealed class UserRepository(AppDbContext db) : BaseRepository<tai_khoan>(db), IUserRepository
{
    protected override IReadOnlyList<Expression<Func<tai_khoan, string?>>> SearchColumns { get; } =
        [u => u.ten_dang_nhap, u => u.ho_ten, u => u.email, u => u.so_dien_thoai];

    protected override QueryColumns<tai_khoan> AllowedColumns => TaiKhoanQueryColumns.Allowed;

    public Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Set.AnyAsync(u => u.ten_dang_nhap == tenDangNhap, cancellationToken);

    public Task<tai_khoan?> FindByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(u => u.ten_dang_nhap == tenDangNhap, cancellationToken);
}
