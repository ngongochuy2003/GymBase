using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.DLAuth.MySql.DLDangNhapNgoai;
using GymBase.DLAuth.MySql.DLPhienDangNhap;
using GymBase.DLAuth.MySql.DLTaiKhoan;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.DLAuth.MySql;

public static class AuthDataExtensions
{
    /// <summary>Đăng ký repository MySQL của nhóm Auth. Thêm repository mới của nhóm thì thêm ở đây.</summary>
    public static IServiceCollection AddAuthData(this IServiceCollection services)
    {
        services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
        services.AddScoped<IPhienDangNhapRepository, PhienDangNhapRepository>();
        services.AddScoped<IDangNhapNgoaiRepository, DangNhapNgoaiRepository>();
        return services;
    }
}
