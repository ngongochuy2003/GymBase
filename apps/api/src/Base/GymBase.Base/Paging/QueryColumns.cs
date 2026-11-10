using System.Linq.Expressions;

namespace GymBase.Base.Paging;

/// <summary>
/// Danh sách cột client được phép sort và filter của một bảng. Chỉ khai báo cột an toàn để lộ ra API,
/// không khai báo cột như mật khẩu hay cờ xóa.
/// </summary>
public sealed class QueryColumns<TEntity>
{
    private readonly Dictionary<string, LambdaExpression> _columns = new(StringComparer.Ordinal);

    /// <summary>Thêm một cột; tên cột là tên property, ví dụ <c>u =&gt; u.ho_ten</c>.</summary>
    public QueryColumns<TEntity> Add<TValue>(Expression<Func<TEntity, TValue>> column)
    {
        var member = column.Body as MemberExpression
            ?? throw new ArgumentException("Cột phải là một property của entity.", nameof(column));
        _columns[member.Member.Name] = column;
        return this;
    }

    internal LambdaExpression? Find(string name) => _columns.GetValueOrDefault(name);
}
