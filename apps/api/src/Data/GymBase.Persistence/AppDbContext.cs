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
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
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
