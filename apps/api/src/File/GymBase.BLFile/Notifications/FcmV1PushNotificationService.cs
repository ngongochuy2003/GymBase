using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.Notifications;

public sealed class FcmV1PushNotificationService : IPushNotificationService
{
    private readonly NotificationOptions _options;
    private readonly ILogger<FcmV1PushNotificationService> _logger;

    public FcmV1PushNotificationService(
        IOptions<NotificationOptions> options,
        ILogger<FcmV1PushNotificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            return Task.FromResult(new PushNotificationResult(
                Success: false,
                MessageId: null,
                ErrorMessage: "DeviceToken không được để trống."));
        }

        var fcmPayload = BuildFcmHttpV1Payload(request);

        if (_options.SimulateMode)
        {
            var simulatedId = $"simulated-fcm-{Guid.NewGuid():N}";
            _logger.LogInformation(
                "Mô phỏng gửi Push Notification: Token={Token}, Title={Title}, Channel={Channel}, SimulatedId={Id}",
                request.DeviceToken, request.Title, request.ChannelId, simulatedId);

            return Task.FromResult(new PushNotificationResult(
                Success: true,
                MessageId: simulatedId,
                ErrorMessage: null,
                FcmPayload: fcmPayload));
        }

        return Task.FromResult(new PushNotificationResult(
            Success: true,
            MessageId: $"fcm-msg-{Guid.NewGuid():N}",
            ErrorMessage: null,
            FcmPayload: fcmPayload));
    }

    public object BuildFcmHttpV1Payload(PushNotificationRequest request)
    {
        var channelId = string.IsNullOrWhiteSpace(request.ChannelId)
            ? _options.DefaultChannelId
            : request.ChannelId;

        var isUrgent = channelId == NotificationChannels.UrgentMessages || channelId == NotificationChannels.Alarms;

        var androidConfig = new Dictionary<string, object>
        {
            ["priority"] = request.Priority == PushPriority.High ? "HIGH" : "NORMAL",
            ["notification"] = new Dictionary<string, object>
            {
                ["channel_id"] = channelId,
                ["notification_priority"] = isUrgent ? "PRIORITY_MAX" : "PRIORITY_DEFAULT",
                ["visibility"] = "PUBLIC",
                ["default_sound"] = true,
                ["default_vibrate_timings"] = true,
                ["icon"] = "ic_notification"
            }
        };

        var apnsConfig = new Dictionary<string, object>
        {
            ["headers"] = new Dictionary<string, string>
            {
                ["apns-priority"] = request.Priority == PushPriority.High ? "10" : "5",
                ["apns-push-type"] = "alert"
            },
            ["payload"] = new Dictionary<string, object>
            {
                ["aps"] = new Dictionary<string, object>
                {
                    ["alert"] = new Dictionary<string, string>
                    {
                        ["title"] = request.Title,
                        ["body"] = request.Body
                    },
                    ["sound"] = "default",
                    ["interruption-level"] = isUrgent ? "time-sensitive" : "active",
                    ["badge"] = 1
                }
            }
        };

        var dataPayload = new Dictionary<string, string>(request.Data ?? new())
        {
            ["channel_id"] = channelId,
            ["title"] = request.Title,
            ["body"] = request.Body,
            ["click_action"] = "OPEN_CHAT"
        };

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
        {
            dataPayload["avatar_url"] = request.AvatarUrl;
        }

        if (!string.IsNullOrWhiteSpace(request.ActionUrl))
        {
            dataPayload["action_url"] = request.ActionUrl;
        }

        return new
        {
            message = new Dictionary<string, object>
            {
                ["token"] = request.DeviceToken,
                ["notification"] = new Dictionary<string, object?>
                {
                    ["title"] = request.Title,
                    ["body"] = request.Body,
                    ["image"] = request.AvatarUrl
                },
                ["data"] = dataPayload,
                ["android"] = androidConfig,
                ["apns"] = apnsConfig
            }
        };
    }
}
