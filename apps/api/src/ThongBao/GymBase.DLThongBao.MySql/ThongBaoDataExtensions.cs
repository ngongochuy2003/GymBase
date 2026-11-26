using GymBase.DLThongBao.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.DLThongBao.MySql;

public static class ThongBaoDataExtensions
{
    /// <summary>Đăng ký repository MySQL của nhóm ThongBao. Thêm repository mới của nhóm thì thêm ở đây.</summary>
    public static IServiceCollection AddThongBaoData(this IServiceCollection services)
    {
        services.AddScoped<IThongBaoRepository, ThongBaoRepository>();
        services.AddScoped<IThietBiRepository, ThietBiRepository>();
        services.AddScoped<ICaiDatThongBaoRepository, CaiDatThongBaoRepository>();
        return services;
    }
}
