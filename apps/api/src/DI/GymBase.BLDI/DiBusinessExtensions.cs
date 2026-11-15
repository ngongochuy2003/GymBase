using GymBase.BLDI.BLDanhMuc;
using GymBase.ModuleContracts.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.BLDI;

public static class DiBusinessExtensions
{
    /// <summary>Đăng ký nghiệp vụ của nhóm DI (Danh mục); repository đăng ký riêng ở <c>Program.cs</c>.</summary>
    public static IServiceCollection AddDIBusiness(this IServiceCollection services)
    {
        services.AddScoped<DanhMucService>();
        services.AddScoped<IDanhMucTraCuu, DanhMucTraCuu>();
        return services;
    }
}
