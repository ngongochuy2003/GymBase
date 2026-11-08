using System.Linq.Expressions;
using GymBase.Base.Paging;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.Contract.Entities;
using GymBase.DLBiz.MySql.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLAuth.MySql;

/// <summary>Truy cập bảng <c>nguoi_dung</c>.</summary>
public sealed class UserRepository(AppDbContext db) : BaseRepository<nguoi_dung>(db), IUserRepository
{
    protected override IReadOnlyList<Expression<Func<nguoi_dung, string?>>> SearchColumns { get; } =
        [u => u.tai_khoan, u => u.ho_ten, u => u.email, u => u.so_dien_thoai];

    protected override QueryColumns<nguoi_dung> AllowedColumns => NguoiDungQueryColumns.Allowed;

    public Task<bool> ExistsByTaiKhoanAsync(string taiKhoan, CancellationToken cancellationToken) =>
        Set.AnyAsync(u => u.tai_khoan == taiKhoan, cancellationToken);
}
