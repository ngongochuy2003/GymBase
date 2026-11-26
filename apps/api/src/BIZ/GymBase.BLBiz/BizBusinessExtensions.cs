using GymBase.Base.Auth;
using GymBase.BLBiz.BaiViet;
using GymBase.BLBiz.BLHoSo;
using GymBase.BLBiz.BLQuyenHan;
using GymBase.BLBiz.BLTruyCap;
using GymBase.ModuleContracts.TepTin;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.BLBiz;

/// <summary>Đăng ký các dịch vụ nghiệp vụ nhóm BIZ vào DI container.</summary>
public static class BizBusinessExtensions
{
    /// <summary>Đăng ký nghiệp vụ của nhóm Biz. Repository của DL.Biz.MySql đăng ký bằng <c>AddBizData</c>.</summary>
    public static IServiceCollection AddBizBusiness(this IServiceCollection services)
    {
        services.AddScoped<BaiVietService>();
        services.AddScoped<IFileTruyCapPolicy, BaiVietFileTruyCapPolicy>();
        services.AddScoped<QuyenHanService>();
        services.AddScoped<IQuyenHanProvider>(provider => provider.GetRequiredService<QuyenHanService>());
        services.AddScoped<TruyCapService>();
        services.AddScoped<HoSoService>();
        return services;
    }
}
