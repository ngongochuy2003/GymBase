using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLAuth.MySql.DLTaiKhoan;

/// <summary>Truy cập bảng <c>tai_khoan</c>.</summary>
public sealed class TaiKhoanRepository(AppDbContext db) : BaseRepository<tai_khoan>(db), ITaiKhoanRepository
{
    protected override IReadOnlyList<Expression<Func<tai_khoan, string?>>> SearchColumns { get; } =
        [u => u.ten_dang_nhap, u => u.ho_ten, u => u.email, u => u.so_dien_thoai];

    protected override QueryColumns<tai_khoan> AllowedColumns => TaiKhoanQueryColumns.Allowed;

    public Task<bool> DLExistsByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Set.AnyAsync(u => u.ten_dang_nhap == tenDangNhap, cancellationToken);

    public Task<bool> DLExistsByEmailAsync(string email, Guid? exceptId, CancellationToken cancellationToken) =>
        Set.AnyAsync(u => u.email == email && u.Id != exceptId, cancellationToken);

    public Task<tai_khoan?> DLFindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(u => u.email == email, cancellationToken);

    public Task<tai_khoan?> DLFindByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(u => u.ten_dang_nhap == tenDangNhap, cancellationToken);
}
