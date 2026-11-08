using GymBase.Base.Auth;
using GymBase.Base.Hosting.Auth;
using GymBase.Base.Hosting.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GymBase.Base.Hosting;

/// <summary>Cấu hình dùng chung của API host, để GymBase.API và test chạy cùng một pipeline.</summary>
public static class AppApiExtensions
{
    /// <summary>Khóa cấu hình chứa danh sách origin được gọi API từ trình duyệt.</summary>
    public const string CorsOriginsSection = "Cors:AllowedOrigins";

    /// <summary>
    /// Đăng ký controller theo kênh, xử lý lỗi, OpenAPI, health check, CORS, URL chữ thường,
    /// <see cref="TimeProvider"/> và <see cref="ICurrentUser"/>.
    /// </summary>
    /// <returns>Builder của MVC, để test thêm controller riêng.</returns>
    /// <exception cref="InvalidOperationException">Danh sách origin có <c>*</c>.</exception>
    public static IMvcBuilder AddAppApi(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = ReadCorsOrigins(configuration);

        services.AddAppErrorHandling();
        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location")));

        return services.AddControllers(options => options.UseApiChannels());
    }

    /// <summary>Dựng pipeline và map endpoint. OpenAPI chỉ bật ở môi trường Development.</summary>
    public static WebApplication UseAppApi(this WebApplication app)
    {
        app.UseAppErrorHandling();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }

    private static string[] ReadCorsOrigins(IConfiguration configuration)
    {
        var origins = (configuration.GetSection(CorsOriginsSection).Get<string[]>() ?? [])
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => origin.Length > 0)
            .ToArray();

        if (origins.Contains("*"))
        {
            throw new InvalidOperationException($"{CorsOriginsSection} không được chứa '*'. Hãy liệt kê từng origin.");
        }

        return origins;
    }
}
