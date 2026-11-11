using System.Collections.Concurrent;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;

namespace GymBase.TestDoubles;

/// <summary>
/// "Bảng" hồ sơ trong bộ nhớ, đăng ký singleton. Hồ sơ có khóa chính là <c>tai_khoan_id</c> nên không dùng
/// <see cref="InMemoryStore{TEntity}"/> (cần <c>Id</c>).
/// </summary>
public sealed class InMemoryHoSoNguoiDungStore
{
    private readonly ConcurrentDictionary<Guid, ho_so_nguoi_dung> _rows = new();

    public void Set(ho_so_nguoi_dung hoSo) => _rows[hoSo.tai_khoan_id] = hoSo;

    public ho_so_nguoi_dung? Find(Guid taiKhoanId) => _rows.GetValueOrDefault(taiKhoanId);
}

/// <summary>Repository hồ sơ giả cho test.</summary>
public sealed class InMemoryHoSoNguoiDungRepository(InMemoryHoSoNguoiDungStore store) : IHoSoNguoiDungRepository
{
    public Task<ho_so_nguoi_dung?> DLFindByTaiKhoanIdAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Find(taiKhoanId));
}
