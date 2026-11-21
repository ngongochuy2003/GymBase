using GymBase.DLFile.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.DLFile.MySql;

public static class FileDataExtensions
{
    /// <summary>Đăng ký repository MySQL của nhóm File. Thêm repository mới của nhóm thì thêm ở đây.</summary>
    public static IServiceCollection AddFileData(this IServiceCollection services)
    {
        services.AddScoped<ITepTinRepository, TepTinRepository>();
        return services;
    }
}
