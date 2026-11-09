namespace GymBase.Poc.Notification.Configuration;

public sealed class NotificationOptions
{
    public const string SectionName = "Notification";

    /// <summary>
    /// Firebase Project ID (dùng cho FCM HTTP v1)
    /// </summary>
    public string FirebaseProjectId { get; set; } = "gymbase-dev";

    /// <summary>
    /// Chế độ mô phỏng trong môi trường POC / Development để chạy không cần key thật
    /// </summary>
    public bool SimulateMode { get; set; } = true;

    /// <summary>
    /// Default channel nếu client không chỉ định
    /// </summary>
    public string DefaultChannelId { get; set; } = "gymbase_urgent_messages";
}
