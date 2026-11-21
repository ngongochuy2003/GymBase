using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.DLDI.MySql.DLDanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.DLDI.MySql;

public static class DiDataExtensions
{
    /// <summary>Đăng ký repository MySQL của nhóm DI (Danh mục). Thêm repository mới của nhóm thì thêm ở đây.</summary>
    public static IServiceCollection AddDIData(this IServiceCollection services)
    {
        services.AddScoped<IDanhMucRepository, DanhMucRepository>();
        return services;
    }
}
