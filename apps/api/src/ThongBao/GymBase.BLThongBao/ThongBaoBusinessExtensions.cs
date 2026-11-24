using GymBase.BLThongBao.BLDayThongBao;
using GymBase.BLThongBao.BLThietBi;
using GymBase.BLThongBao.BLThongBao;
using GymBase.ModuleContracts.ThongBao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymBase.BLThongBao;

public static class ThongBaoBusinessExtensions
{
    /// <summary>Đăng ký nghiệp vụ thông báo và worker đẩy push. Repository MySQL đăng ký riêng bằng <c>AddThongBaoData</c>.</summary>
    public static IServiceCollection AddThongBaoBusiness(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ThongBaoOptions>(configuration.GetSection(ThongBaoOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ThongBaoService>();
        services.AddScoped<IThongBaoTao>(sp => sp.GetRequiredService<ThongBaoService>());
        services.AddScoped<ThietBiService>();

        // TODO(THONGBAO): thêm FcmPushSender khi đã có Firebase project và service account (ThongBao:PushProvider = FCM).
        //   Hiện chỉ có LogPushSender nên push chưa tới máy thật.
        services.AddSingleton<IPushSender, LogPushSender>();
        services.AddScoped<ThongBaoDayService>();
        services.AddHostedService<ThongBaoDayWorker>();

        return services;
    }
}
