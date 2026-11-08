using System.Reflection;
using FluentValidation;
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
    /// <see cref="TimeProvider"/>, <see cref="ICurrentUser"/> và các validator.
    /// </summary>
    /// <param name="validatorAssemblies">Các assembly BL chứa validator.</param>
    /// <returns>Builder của MVC, để test thêm controller riêng.</returns>
    /// <exception cref="InvalidOperationException">Danh sách origin có <c>*</c>.</exception>
    public static IMvcBuilder AddAppApi(
        this IServiceCollection services, IConfiguration configuration, params Assembly[] validatorAssemblies)
    {
        var origins = ReadCorsOrigins(configuration);

        services.AddValidatorsFromAssemblies(validatorAssemblies);

        services.AddAppErrorHandling();
        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddAppAuthentication(configuration);
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location")));

        return services.AddControllers(options =>
        {
            options.UseApiChannels();

            // Kiểm tra dữ liệu do FluentValidation làm trong BaseService. Không tắt thì MVC tự coi mọi
            // string không-null là bắt buộc và trả lỗi tiếng Anh trước khi vào service.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        });
    }

    /// <summary>Dựng pipeline và map endpoint. OpenAPI chỉ bật ở môi trường Development.</summary>
    public static WebApplication UseAppApi(this WebApplication app)
    {
        app.UseAppErrorHandling();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
        }

        app.MapControllers();
        app.MapHealthChecks("/health").AllowAnonymous();

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
