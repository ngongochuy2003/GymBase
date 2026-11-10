namespace GymBase.BLFile.Notifications;

public static class NotificationChannels
{
    public const string UrgentMessages = "gymbase_urgent_messages";
    public const string Alarms = "gymbase_alarms";
    public const string General = "gymbase_general";
}

public enum PushPriority
{
    Normal,
    High
}

public sealed record PushNotificationRequest(
    string DeviceToken,
    string Title,
    string Body,
    string ChannelId = NotificationChannels.UrgentMessages,
    PushPriority Priority = PushPriority.High,
    string? AvatarUrl = null,
    string? ActionUrl = null,
    Dictionary<string, string>? Data = null
);

public sealed record PushNotificationResult(
    bool Success,
    string? MessageId,
    string? ErrorMessage,
    object? FcmPayload = null
);
