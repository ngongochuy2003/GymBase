using GymBase.Base.Logging;
using GymBase.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace GymBase.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>Phiên bản MySQL đã chốt (Q5). Khai báo cố định để không phải kết nối DB mới dựng được model.</summary>
    private static readonly MySqlServerVersion ServerVersion = new(new Version(8, 4, 0));

    private const string RetentionDaysKey = "AuditLog:RetentionDays";
    private const int DefaultRetentionDays = 90;

    /// <summary>
    /// Đăng ký <see cref="AppDbContext"/> (scoped theo request) và health check DB.
    /// </summary>
    /// <exception cref="InvalidOperationException">Chưa cấu hình <c>ConnectionStrings:DefaultConnection</c>.</exception>
    public static IServiceCollection AddAppPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình ConnectionStrings:DefaultConnection. Khi dev, đặt bằng dotnet user-secrets (xem docs/api/cach-chay-api.md).");
        }

        services.AddDbContext<AppDbContext>(options => ConfigureMySql(options, connectionString));
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("mysql");

        services.AddSingleton(new SystemLogRetention(ReadRetentionDays(configuration)));
        services.AddSingleton<IAuditLogWriter, SystemLogWriter>();
        services.AddHostedService<SystemLogCleanupService>();

        return services;
    }

    /// <summary>Từ chối giá trị không dương, vì 0 hoặc số âm sẽ xóa sạch audit log.</summary>
    private static int ReadRetentionDays(IConfiguration configuration)
    {
        var value = configuration[RetentionDaysKey];
        if (value is null)
        {
            return DefaultRetentionDays;
        }

        return int.TryParse(value, out var days) && days > 0
            ? days
            : throw new InvalidOperationException($"{RetentionDaysKey} phải là số nguyên lớn hơn 0.");
    }

    /// <summary>Cấu hình provider MySQL dùng chung cho app và test kiểm tra model.</summary>
    public static void ConfigureMySql(DbContextOptionsBuilder options, string connectionString)
    {
        // Binary16 giữ thứ tự byte của UUID, nên UUIDv7 vẫn tăng dần trong index (Q6).
        var connection = new MySqlConnectionStringBuilder(connectionString) { GuidFormat = MySqlGuidFormat.Binary16 };

        options
            .UseMySql(connection.ConnectionString, ServerVersion, mysql => mysql.EnableRetryOnFailure())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
