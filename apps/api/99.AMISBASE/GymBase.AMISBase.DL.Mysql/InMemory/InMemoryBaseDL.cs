using GymBase.AMISBase.DL.Contract;
using GymBase.AMISBase.DL.Contract.Entities;
using GymBase.AMISBase.DL.Contract.Paging;

namespace GymBase.AMISBase.DL.Mysql.InMemory;

// TODO(DB): thêm BaseDL<TEntity> chạy SQL (Dapper + MySqlConnector), DL từng bảng đổi lớp cha sang đó;
//   xóa thư mục InMemory khi không còn bảng nào dùng. Mục 3, docs/api/viec-con-lai-base.md.
/// <summary>
/// MOCK: hiện thực <see cref="IBaseDL{TEntity}"/> trên <see cref="InMemoryStore{TEntity}"/>, dùng tạm đến khi
/// nối MySQL (mục 3 trong docs/api/viec-con-lai-base.md). Khi có DB thì thêm <c>BaseDL&lt;TEntity&gt;</c> chạy SQL
/// và đổi lớp cha của DL từng bảng. Hành vi (ẩn xóa mềm, tìm kiếm, mới tạo trước, phân trang)
/// phải giữ nguyên để BL và API không phải sửa.
/// </summary>
public abstract class InMemoryBaseDL<TEntity>(InMemoryStore<TEntity> store) : IBaseDL<TEntity>
    where TEntity : BaseEntity
{
    protected InMemoryStore<TEntity> Store { get; } = store;

    /// <summary>
    /// Các giá trị được so khớp với từ khóa tìm kiếm (chứa chuỗi, không phân biệt hoa thường).
    /// Mặc định null: bảng không hỗ trợ tìm kiếm, từ khóa bị bỏ qua.
    /// </summary>
    protected virtual IEnumerable<string?>? GetSearchValues(TEntity entity) => null;

    public Task<PagingResult<TEntity>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var query = Store.All().Where(IsVisible);

        if (request.Search is { Length: > 0 } search)
        {
            query = query.Where(e => GetSearchValues(e) is not { } values
                || values.Any(v => v is not null && v.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var filtered = query
            .OrderByDescending(e => e is BaseAuditableEntity auditable ? auditable.ngay_tao : DateTime.MinValue)
            .ThenByDescending(e => e.Id)
            .ToList();
        var page = filtered.Skip(request.Skip).Take(request.PageSize).ToList();

        return Task.FromResult(new PagingResult<TEntity>(page, request.Page, request.PageSize, filtered.Count));
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = Store.Find(id);
        return Task.FromResult(entity is not null && IsVisible(entity) ? entity : null);
    }

    public Task InsertAsync(TEntity entity, CancellationToken cancellationToken) =>
        Store.TryAdd(entity)
            ? Task.CompletedTask
            : throw new InvalidOperationException($"Đã tồn tại {typeof(TEntity).Name} với Id {entity.Id}.");

    public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var current = Store.Find(entity.Id);
        return Task.FromResult(current is not null && IsVisible(current) && Store.TryReplace(entity));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Store.TryRemove(id));

    private static bool IsVisible(TEntity entity) => entity is not ISoftDelete { is_deleted: true };
}
