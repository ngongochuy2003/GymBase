using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GymBase.Base.Hosting.Logging;

/// <summary>Đăng ký log kỹ thuật. Tách khỏi AddAppApi để test không ghi file log.</summary>
public static class LoggingExtensions
{
    /// <summary>Thay logger mặc định bằng Serilog, đọc đích ghi và mức log từ mục <c>Serilog</c> của cấu hình.</summary>
    public static IServiceCollection AddAppLogging(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSerilog(
            logger => logger.ReadFrom.Configuration(configuration).Enrich.FromLogContext(),
            preserveStaticLogger: true);
}
