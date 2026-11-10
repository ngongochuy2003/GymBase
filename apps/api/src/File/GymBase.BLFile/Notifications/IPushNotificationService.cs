namespace GymBase.BLFile.Notifications;

public interface IPushNotificationService
{
    Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken ct = default);
    object BuildFcmHttpV1Payload(PushNotificationRequest request);
}
