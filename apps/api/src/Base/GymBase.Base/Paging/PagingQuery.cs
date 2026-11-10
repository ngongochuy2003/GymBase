using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;

namespace GymBase.Base.Paging;

/// <summary>
/// Đổi tham số <c>sort</c>, <c>filter</c> của client thành biểu thức LINQ. Tên cột chỉ dùng để tra trong
/// <see cref="QueryColumns{TEntity}"/>. Giá trị truyền qua closure như LINQ thường, EF và provider MySQL
/// lo tham số hóa hoặc escape, nên không ghép chuỗi của client vào SQL.
/// </summary>
/// <exception cref="ValidationFailedException">Sai cú pháp, cột ngoài danh sách, hoặc giá trị sai kiểu.</exception>
public static class PagingQuery
{
    private const string SortField = "sort";
    private const string FilterField = "filter";

    private static readonly MethodInfo CaptureMethod =
        typeof(PagingQuery).GetMethod(nameof(Capture), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    /// <summary>
    /// Sắp xếp theo dạng <c>-ngay_tao,ho_ten</c> (dấu <c>-</c> là giảm dần). Luôn thêm Id giảm dần
    /// ở cuối để thứ tự giữa các trang ổn định.
    /// </summary>
    public static IQueryable<TEntity> ApplySort<TEntity>(
        IQueryable<TEntity> query, string sort, QueryColumns<TEntity> columns)
        where TEntity : BaseEntity
    {
        IOrderedQueryable<TEntity>? ordered = null;

        foreach (var part in sort.Split(',', StringSplitOptions.TrimEntries))
        {
            var descending = part.StartsWith('-');
            var name = descending ? part[1..] : part;
            var column = columns.Find(name) ?? throw Invalid(SortField, $"Không được sắp xếp theo cột '{name}'.");
            var method = (ordered is null ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty);

            ordered = (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(Expression.Call(
                typeof(Queryable),
                method,
                [typeof(TEntity), column.ReturnType],
                (ordered ?? query).Expression,
                Expression.Quote(column)));
        }

        return ordered!.ThenByDescending(e => e.Id);
    }

    /// <summary>
    /// Lọc theo dạng <c>cot:toan_tu:gia_tri</c>, nhiều điều kiện cách nhau bằng dấu phẩy và kết hợp AND.
    /// Toán tử: <c>eq</c>, <c>ne</c>; <c>gt</c>, <c>gte</c>, <c>lt</c>, <c>lte</c> cho số và thời gian;
    /// <c>contains</c> cho chuỗi. Giá trị không được chứa dấu phẩy.
    /// </summary>
    public static IQueryable<TEntity> ApplyFilter<TEntity>(
        IQueryable<TEntity> query, string filter, QueryColumns<TEntity> columns)
    {
        foreach (var condition in filter.Split(',', StringSplitOptions.TrimEntries))
        {
            var parts = condition.Split(':', 3, StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                throw Invalid(FilterField, $"Điều kiện lọc '{condition}' phải có dạng cot:toan_tu:gia_tri.");
            }

            var column = columns.Find(parts[0]) ?? throw Invalid(FilterField, $"Không được lọc theo cột '{parts[0]}'.");
            var predicate = BuildComparison(column, parts[0], parts[1], parts[2]);
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(predicate, column.Parameters[0]));
        }

        return query;
    }

    private static BinaryExpression BuildComparison(LambdaExpression column, string name, string op, string raw)
    {
        var columnType = column.ReturnType;
        var valueType = Nullable.GetUnderlyingType(columnType) ?? columnType;
        var isSupported = op switch
        {
            "eq" or "ne" => true,
            "gt" or "gte" or "lt" or "lte" => valueType != typeof(string) && valueType != typeof(bool) && valueType != typeof(Guid),
            "contains" => valueType == typeof(string),
            _ => false,
        };

        if (!isSupported)
        {
            throw Invalid(FilterField, $"Cột '{name}' không dùng được toán tử '{op}'.");
        }

        var value = ParseValue(valueType, raw) ?? throw Invalid(FilterField, $"Giá trị '{raw}' của cột '{name}' không hợp lệ.");
        var parameter = (Expression)CaptureMethod.MakeGenericMethod(columnType).Invoke(null, [value])!;
        var body = column.Body;

        return op switch
        {
            "eq" => Expression.Equal(body, parameter),
            "ne" => Expression.NotEqual(body, parameter),
            "gt" => Expression.GreaterThan(body, parameter),
            "gte" => Expression.GreaterThanOrEqual(body, parameter),
            "lt" => Expression.LessThan(body, parameter),
            "lte" => Expression.LessThanOrEqual(body, parameter),
            _ => Expression.AndAlso(
                Expression.NotEqual(body, Expression.Constant(null, typeof(string))),
                Expression.Call(body, ContainsMethod, parameter)),
        };
    }

    /// <summary>Đưa giá trị vào closure, để EF xử lý y như biến trong câu LINQ viết tay.</summary>
    private static Expression Capture<TValue>(TValue value)
    {
        Expression<Func<TValue>> closure = () => value;
        return closure.Body;
    }

    private static object? ParseValue(Type type, string raw)
    {
        var culture = CultureInfo.InvariantCulture;

        if (type == typeof(string)) return raw;
        if (type == typeof(bool)) return bool.TryParse(raw, out var b) ? b : null;
        if (type == typeof(int)) return int.TryParse(raw, NumberStyles.Integer, culture, out var i) ? i : null;
        if (type == typeof(long)) return long.TryParse(raw, NumberStyles.Integer, culture, out var l) ? l : null;
        if (type == typeof(decimal)) return decimal.TryParse(raw, NumberStyles.Number, culture, out var m) ? m : null;
        if (type == typeof(double)) return double.TryParse(raw, NumberStyles.Float, culture, out var d) ? d : null;
        if (type == typeof(Guid)) return Guid.TryParse(raw, out var g) ? g : null;
        if (type == typeof(DateTime))
        {
            // Mọi thời điểm là UTC (Q7): chuỗi không có múi giờ được hiểu là UTC.
            return DateTime.TryParse(raw, culture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var t) ? t : null;
        }

        throw new InvalidOperationException($"Chưa hỗ trợ lọc kiểu {type.Name}.");
    }

    private static ValidationFailedException Invalid(string field, string message) => new(field, message);
}
