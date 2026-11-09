namespace GymBase.Poc.Notification.Contracts;

public static class NotificationChannels
{
    /// <summary>
    /// Channel tin nhắn khẩn cấp (PT nhắn học viên, đổi lịch tập).
    /// Android: IMPORTANCE_MAX, sáng màn hình khóa, rung chuông to.
    /// </summary>
    public const string UrgentMessages = "gymbase_urgent_messages";

    /// <summary>
    /// Channel báo thức (nhắc giờ tập, giờ uống nước, giờ dinh dưỡng).
    /// Android: IMPORTANCE_MAX + WAKE_LOCK + chuông liên tục.
    /// </summary>
    public const string Alarms = "gymbase_alarms";

    /// <summary>
    /// Channel thông thường (bài viết mới, thông tin khuyến mãi).
    /// </summary>
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
