using System.Collections.Concurrent;
using System.Text.Json;
using GymBase.Base.Entities;

namespace GymBase.TestDoubles;

/// <summary>
/// MOCK: "bảng" trong bộ nhớ cho một loại entity, đăng ký singleton để dữ liệu sống qua các request.
/// Mất dữ liệu khi tắt host. Luôn trả bản sao, để BL sửa object mà chưa gọi Update thì dữ liệu
/// không đổi, giống khi đọc từ DB thật.
/// </summary>
public sealed class InMemoryStore<TEntity> where TEntity : BaseEntity
{
    private readonly ConcurrentDictionary<Guid, TEntity> _rows = new();
    private readonly Lock _seedLock = new();
    private bool _seeded;

    /// <summary>Nạp dữ liệu mẫu đúng một lần cho cả vòng đời store.</summary>
    public void SeedOnce(Func<IEnumerable<TEntity>> seed)
    {
        lock (_seedLock)
        {
            if (_seeded)
            {
                return;
            }

            foreach (var entity in seed())
            {
                _rows[entity.Id] = Clone(entity);
            }

            _seeded = true;
        }
    }

    public IReadOnlyList<TEntity> All() => _rows.Values.Select(Clone).ToList();

    public TEntity? Find(Guid id) => _rows.TryGetValue(id, out var entity) ? Clone(entity) : null;

    public bool TryAdd(TEntity entity) => _rows.TryAdd(entity.Id, Clone(entity));

    public bool TryReplace(TEntity entity) =>
        _rows.TryGetValue(entity.Id, out var current) && _rows.TryUpdate(entity.Id, Clone(entity), current);

    public bool TryRemove(Guid id) => _rows.TryRemove(id, out _);

    private static TEntity Clone(TEntity entity) =>
        JsonSerializer.Deserialize<TEntity>(JsonSerializer.Serialize(entity))!;
}
