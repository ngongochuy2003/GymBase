using System.Collections.Concurrent;

namespace GymBase.Poc.Media.Model;

public interface ITepTinRepository
{
    Task<TepTin?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TepTin>> GetByNguoiTaiLenIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TepTin>> GetPendingMigrationAsync(string fromProvider, int limit = 100, CancellationToken ct = default);
    Task CreateAsync(TepTin tepTin, CancellationToken ct = default);
    Task UpdateAsync(TepTin tepTin, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Triển khai ITepTinRepository trong bộ nhớ phục vụ POC độc lập.
/// Sẵn sàng thay thế bằng EF Core DbSet&lt;TepTin&gt; khi ghép vào GymBase.DL.
/// </summary>
public sealed class InMemoryTepTinRepository : ITepTinRepository
{
    private readonly ConcurrentDictionary<Guid, TepTin> _store = new();

    public Task<TepTin?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var tepTin);
        return Task.FromResult(tepTin);
    }

    public Task<IReadOnlyList<TepTin>> GetByNguoiTaiLenIdAsync(Guid userId, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(t => t.NguoiTaiLenId == userId && t.NgayXoa == null)
            .OrderByDescending(t => t.NgayTao)
            .ToList();
        return Task.FromResult<IReadOnlyList<TepTin>>(list);
    }

    public Task<IReadOnlyList<TepTin>> GetPendingMigrationAsync(string fromProvider, int limit = 100, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(t => t.NhaCungCap == fromProvider && t.TrangThai == "READY" && t.NgayXoa == null)
            .Take(limit)
            .ToList();
        return Task.FromResult<IReadOnlyList<TepTin>>(list);
    }

    public Task CreateAsync(TepTin tepTin, CancellationToken ct = default)
    {
        _store[tepTin.Id] = tepTin;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TepTin tepTin, CancellationToken ct = default)
    {
        tepTin.NgayCapNhat = DateTime.UtcNow;
        _store[tepTin.Id] = tepTin;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var tepTin))
        {
            tepTin.NgayXoa = DateTime.UtcNow;
            tepTin.TrangThai = "DELETED";
        }
        return Task.CompletedTask;
    }
}
