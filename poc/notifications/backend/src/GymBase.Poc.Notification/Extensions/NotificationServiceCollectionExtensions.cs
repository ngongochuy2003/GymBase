using GymBase.Poc.Notification.Configuration;
using GymBase.Poc.Notification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.Poc.Notification.Extensions;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddGymBaseNotifications(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<NotificationOptions>? configureOptions = null)
    {
        var section = configuration.GetSection(NotificationOptions.SectionName);
        services.Configure<NotificationOptions>(section);

        if (configureOptions != null)
        {
            services.PostConfigure(configureOptions);
        }

        services.AddSingleton<IPushNotificationService, FcmV1PushNotificationService>();

        return services;
    }
}
