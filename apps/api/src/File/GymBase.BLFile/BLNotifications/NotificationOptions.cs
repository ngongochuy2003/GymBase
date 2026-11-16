namespace GymBase.BLFile.BLNotifications;

public sealed class NotificationOptions
{
    public const string SectionName = "Notification";

    public string FirebaseProjectId { get; set; } = "gymbase-dev";
    public bool SimulateMode { get; set; } = true;
    public string DefaultChannelId { get; set; } = "gymbase_urgent_messages";
}
