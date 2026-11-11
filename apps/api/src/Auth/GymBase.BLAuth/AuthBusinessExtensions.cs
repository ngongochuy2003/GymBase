using GymBase.Base.Auth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLAuth.BLXacThuc;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.BLAuth;

public static class AuthBusinessExtensions
{
    /// <summary>Số vòng PBKDF2 theo khuyến nghị OWASP; mặc định của ASP.NET chỉ 100.000.</summary>
    public const int PasswordHashIterations = 600_000;

    /// <summary>Đăng ký nghiệp vụ của nhóm Auth.</summary>
    public static IServiceCollection AddAuthBusiness(this IServiceCollection services)
    {
        services.Configure<PasswordHasherOptions>(options => options.IterationCount = PasswordHashIterations);
        services.AddSingleton<IPasswordHasher<tai_khoan>, PasswordHasher<tai_khoan>>();
        services.AddSingleton<LoginThrottle>();
        services.AddScoped<TaiKhoanService>();
        services.AddScoped<XacThucService>();
        services.AddScoped<ITaiKhoanTrangThaiProvider, TaiKhoanTrangThaiService>();
        return services;
    }
}
