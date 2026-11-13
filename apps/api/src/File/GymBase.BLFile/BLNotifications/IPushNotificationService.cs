namespace GymBase.BLFile.BLNotifications;

public interface IPushNotificationService
{
    Task<PushNotificationResult> BLSendAsync(PushNotificationRequest request, CancellationToken ct = default);
    object BLBuildFcmHttpV1Payload(PushNotificationRequest request);
}
