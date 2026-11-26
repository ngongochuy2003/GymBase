using System.Text.Json;
using GymBase.BLThongBao.BLDayThongBao;
using GymBase.BLThongBao.BLThietBi;
using GymBase.BLThongBao.BLThongBao;
using GymBase.ModuleContracts.ThongBao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

        AddPushSender(services, configuration);
        services.AddScoped<ThongBaoDayService>();
        services.AddHostedService<ThongBaoDayWorker>();

        return services;
    }

    /// <summary>
    /// Chọn cách gửi push theo <c>ThongBao:PushProvider</c>: <c>LOG</c> (mặc định, chỉ ghi log) hoặc <c>FCM</c>.
    /// Cấu hình FCM thiếu hoặc sai thì dừng ngay lúc khởi động, không đợi tới lượt gửi đầu tiên mới lỗi.
    /// </summary>
    private static void AddPushSender(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ThongBaoOptions.SectionName).Get<ThongBaoOptions>() ?? new ThongBaoOptions();

        if (options.PushProvider.Equals("LOG", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IPushSender, LogPushSender>();
            return;
        }

        if (!options.PushProvider.Equals("FCM", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"ThongBao:PushProvider phải là LOG hoặc FCM, đang là '{options.PushProvider}'.");
        }

        var path = options.Fcm.CredentialsPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            throw new InvalidOperationException(
                "ThongBao:PushProvider = FCM nhưng ThongBao:Fcm:CredentialsPath trống hoặc không có file. " +
                "Đặt đường dẫn tới file service account bằng dotnet user-secrets (xem docs/onboarding/thong-bao-fcm.md).");
        }

        // Bỏ trống ProjectId thì lấy project_id trong chính file service account.
        var projectId = options.Fcm.ProjectId;
        if (string.IsNullOrWhiteSpace(projectId))
        {
            projectId = DocProjectId(path);
            services.PostConfigure<ThongBaoOptions>(o => o.Fcm.ProjectId = projectId);
        }

        services.AddSingleton<IFcmAccessTokenProvider>(sp =>
            new GoogleFcmAccessTokenProvider(sp.GetRequiredService<IOptions<ThongBaoOptions>>().Value.Fcm.CredentialsPath));
        services.AddHttpClient<IPushSender, FcmPushSender>(client =>
        {
            client.BaseAddress = new Uri("https://fcm.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
    }

    private static string DocProjectId(string credentialsPath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(credentialsPath));
        return doc.RootElement.TryGetProperty("project_id", out var id) && id.GetString() is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException("File service account không có project_id; đặt ThongBao:Fcm:ProjectId.");
    }
}
