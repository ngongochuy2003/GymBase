using System.Linq.Expressions;
using System.Reflection;
using GymBase.Base.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymBase.DLBiz.MySql.Persistence;

/// <summary>Các assembly <c>DL*.MySql</c> chứa <see cref="IEntityTypeConfiguration{TEntity}"/>, do host truyền vào.</summary>
public sealed record EntityConfigurationAssemblies(IReadOnlyList<Assembly> Assemblies);

/// <summary>
/// DbContext duy nhất của cả database. Bảng của từng nhóm được khai báo bằng
/// <see cref="IEntityTypeConfiguration{TEntity}"/> trong <c>DL*.MySql</c> của nhóm đó.
/// Bản ghi <see cref="ISoftDelete"/> đã xóa mềm tự động bị ẩn khỏi mọi truy vấn.
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    EntityConfigurationAssemblies configurationAssemblies) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var assembly in configurationAssemblies.Assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

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
