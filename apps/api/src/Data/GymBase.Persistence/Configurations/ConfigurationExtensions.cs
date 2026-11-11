using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using GymBase.Base.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymBase.Persistence.Configurations;

/// <summary>
/// Các bước cấu hình lặp lại ở nhiều bảng: enum lưu chuỗi kèm <c>CHECK</c>, cột JSON, khóa ngoại <c>RESTRICT</c>,
/// cột sinh tự động để thay partial unique index (MySQL không có).
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>Collation phân biệt hoa thường, cho cột chứa khóa S3 hoặc token mà so sánh mặc định sẽ gộp nhầm.</summary>
    public const string BinaryCollation = "utf8mb4_bin";

    /// <summary>Độ dài cột nguoi_tao và nguoi_chinh_sua, khớp bảng tai_khoan.</summary>
    private const int AuditUserLength = 100;

    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    /// <summary>Cột enum: lưu tên thành viên (snake_case) dạng chuỗi, giới hạn độ dài.</summary>
    public static PropertyBuilder<T> AsEnumString<T>(this PropertyBuilder<T> property, int maxLength) =>
        property.HasConversion<string>().HasMaxLength(maxLength);

    /// <summary>
    /// Cột JSON ánh xạ tới kiểu .NET (danh sách). Enum trong JSON lưu theo tên. Cột kiểu <c>json</c> của MySQL.
    /// </summary>
    public static PropertyBuilder<T> AsJson<T>(this PropertyBuilder<T> property)
        where T : class?
    {
        property.HasColumnType("json");
        property.HasConversion(
            new ValueConverter<T, string>(
                value => JsonSerializer.Serialize(value, Json),
                text => JsonSerializer.Deserialize<T>(text, Json)!),
            new ValueComparer<T>(
                (a, b) => JsonSerializer.Serialize(a, Json) == JsonSerializer.Serialize(b, Json),
                value => JsonSerializer.Serialize(value, Json).GetHashCode(),
                value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!));
        return property;
    }

    /// <summary>Cột JSON giữ nguyên chuỗi, service tự đọc và kiểm tra theo schema.</summary>
    public static PropertyBuilder<T> AsJsonText<T>(this PropertyBuilder<T> property) =>
        property.HasColumnType("json");

    /// <summary>Cột văn bản dài vừa phải (tối đa 64 KB), tương ứng <c>text</c> của tài liệu thiết kế.</summary>
    public static PropertyBuilder<T> AsText<T>(this PropertyBuilder<T> property) =>
        property.HasColumnType("text");

    /// <summary>Chỉ cho các giá trị của enum trong cột chuỗi (<c>CHECK</c>).</summary>
    public static TableBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(this TableBuilder<TEntity> table, Expression<Func<TEntity, TEnum>> column)
        where TEntity : class
        where TEnum : struct, Enum =>
        AddEnumCheck(table, MemberName(column.Body), typeof(TEnum));

    /// <summary>Như trên, cho cột enum cho phép NULL (NULL luôn qua <c>CHECK</c>).</summary>
    public static TableBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(this TableBuilder<TEntity> table, Expression<Func<TEntity, TEnum?>> column)
        where TEntity : class
        where TEnum : struct, Enum =>
        AddEnumCheck(table, MemberName(column.Body), typeof(TEnum));

    /// <summary>Khóa ngoại <c>RESTRICT</c>: xóa bản ghi cha không kéo theo xóa con; bản ghi cha đang được tham chiếu thì không xóa được.</summary>
    public static void HasRestrictForeignKey<TEntity>(this EntityTypeBuilder<TEntity> builder, Type principal, Expression<Func<TEntity, object?>> foreignKey)
        where TEntity : class =>
        builder.HasOne(principal).WithMany().HasForeignKey(MemberName(foreignKey.Body)).OnDelete(DeleteBehavior.Restrict);

    /// <summary>
    /// Thay partial unique index của PostgreSQL: cột sinh tự động bằng <paramref name="sql"/> (NULL khi không thuộc
    /// phạm vi ràng buộc) kèm unique index. MySQL cho phép nhiều NULL trong unique index.
    /// </summary>
    public static void HasUniqueComputedColumn<TEntity>(this EntityTypeBuilder<TEntity> builder, string column, int maxLength, string sql)
        where TEntity : class
    {
        builder.Property<string?>(column).HasMaxLength(maxLength).HasComputedColumnSql(sql, stored: false);
        builder.HasIndex(column).IsUnique();
    }

    /// <summary>Độ dài hai cột người tạo, người sửa của <see cref="BaseAuditableEntity"/>.</summary>
    public static void ConfigureAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseAuditableEntity
    {
        builder.Property(e => e.nguoi_tao).HasMaxLength(AuditUserLength);
        builder.Property(e => e.nguoi_chinh_sua).HasMaxLength(AuditUserLength);
    }

    private static TableBuilder<TEntity> AddEnumCheck<TEntity>(TableBuilder<TEntity> table, string column, Type enumType)
        where TEntity : class
    {
        var values = string.Join(", ", Enum.GetNames(enumType).Select(name => $"'{name}'"));
        table.HasCheckConstraint($"CK_{typeof(TEntity).Name}_{column}", $"`{column}` IN ({values})");
        return table;
    }

    private static string MemberName(Expression body) => body switch
    {
        UnaryExpression { Operand: var operand } => MemberName(operand),
        MemberExpression member => member.Member.Name,
        _ => throw new ArgumentException($"Cần biểu thức chọn một property, nhận được: {body}"),
    };
}
