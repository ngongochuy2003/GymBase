using GymBase.BLBiz.BaiViet;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.BLBiz;

/// <summary>Đăng ký các dịch vụ nghiệp vụ nhóm BIZ vào DI container.</summary>
public static class BizBusinessExtensions
{
    public static IServiceCollection AddBizBusiness(this IServiceCollection services)
    {
        services.AddScoped<BaiVietService>();
        return services;
    }
}
