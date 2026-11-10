using System.Linq.Expressions;
using GymBase.Base.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymBase.Persistence;

/// <summary>
/// DbContext duy nhất của cả database. Bảng của từng nhóm được khai báo bằng
/// <see cref="IEntityTypeConfiguration{TEntity}"/> trong thư mục <c>Configurations/&lt;Nhóm&gt;</c>.
/// Bản ghi <see cref="ISoftDelete"/> đã xóa mềm tự động bị ẩn khỏi mọi truy vấn.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Độ dài UUID dạng chữ, ví dụ <c>0193f0a0-0000-7000-8000-000000000001</c>.</summary>
    private const int GuidTextLength = 36;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();

        // Mọi cột Guid lưu dạng varchar(36), đọc được trực tiếp trong công cụ DB (quyết định Q6).
        configurationBuilder.Properties<Guid>().HaveConversion<GuidToStringConverter>().HaveMaxLength(GuidTextLength);
        configurationBuilder.Properties<Guid?>().HaveConversion<GuidToStringConverter>().HaveMaxLength(GuidTextLength);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType)))
        {
            var row = Expression.Parameter(entityType.ClrType, "e");
            var notDeleted = Expression.Not(Expression.Property(row, nameof(ISoftDelete.is_deleted)));
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(notDeleted, row));
        }
    }

    /// <summary>MySQL không lưu DateTimeKind; mọi thời điểm trong DB đều là UTC (quyết định Q7).</summary>
    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
