using System.Linq.Expressions;
using GymBase.Base.Crud;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MySqlConnector;

namespace GymBase.DLBiz.MySql.Persistence;

/// <summary>
/// Hiện thực <see cref="IBaseRepository{TEntity}"/> bằng EF Core. DL của từng bảng kế thừa lớp này.
/// Mỗi thao tác ghi lưu ngay xuống DB và không giữ entity trong change tracker.
/// </summary>
public abstract class BaseRepository<TEntity>(AppDbContext db) : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected AppDbContext Db { get; } = db;

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    /// <summary>
    /// Các cột được so khớp với từ khóa tìm kiếm (chứa chuỗi, theo collation của cột).
    /// Mặc định rỗng: bảng không hỗ trợ tìm kiếm, từ khóa bị bỏ qua.
    /// </summary>
    protected virtual IReadOnlyList<Expression<Func<TEntity, string?>>> SearchColumns => [];

    public async Task<PagingResult<TEntity>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        IQueryable<TEntity> query = Set;

        if (request.search is { Length: > 0 } search && SearchColumns.Count > 0)
        {
            query = query.Where(MatchesSearch(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await NewestFirst(query).Skip(request.Skip).Take(request.page_size).ToListAsync(cancellationToken);

        return new PagingResult<TEntity>(rows, request.page, request.page_size, total);
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    /// <exception cref="ConflictException">Trùng khóa chính hoặc một unique index.</exception>
    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken)
    {
        Set.Add(entity);
        await SaveAsync(entity, cancellationToken);
    }

    /// <exception cref="ConflictException">Giá trị mới trùng một unique index.</exception>
    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (!await Set.AnyAsync(e => e.Id == entity.Id, cancellationToken))
        {
            return false;
        }

        Set.Update(entity);
        try
        {
            await SaveAsync(entity, cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false; // Bản ghi vừa bị xóa bởi request khác
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await Set.IgnoreQueryFilters().Where(e => e.Id == id).ExecuteDeleteAsync(cancellationToken) > 0;

    private async Task SaveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            await Db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
        {
            throw new ConflictException();
        }
        finally
        {
            Db.Entry(entity).State = EntityState.Detached;
        }
    }

    private static IQueryable<TEntity> NewestFirst(IQueryable<TEntity> query) =>
        typeof(BaseAuditableEntity).IsAssignableFrom(typeof(TEntity))
            ? query.OrderByDescending(e => EF.Property<DateTime>(e, nameof(BaseAuditableEntity.ngay_tao))).ThenByDescending(e => e.Id)
            : query.OrderByDescending(e => e.Id);

    /// <summary>Dựng điều kiện <c>cot1 LIKE %search% OR cot2 LIKE %search%...</c>; search luôn là tham số SQL.</summary>
    private Expression<Func<TEntity, bool>> MatchesSearch(string search)
    {
        var row = Expression.Parameter(typeof(TEntity), "e");
        Expression<Func<string>> keyword = () => search;
        var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

        var condition = SearchColumns
            .Select(column => (Expression)Expression.Call(
                ReplacingExpressionVisitor.Replace(column.Parameters[0], row, column.Body),
                contains,
                keyword.Body))
            .Aggregate(Expression.OrElse);

        return Expression.Lambda<Func<TEntity, bool>>(condition, row);
    }
}
