using GymBase.Poc.Notification.Contracts;

namespace GymBase.Poc.Notification.Services;

public interface IPushNotificationService
{
    /// <summary>
    /// Gửi thông báo đẩy tới thiết bị người dùng
    /// </summary>
    Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sinh cấu trúc Payload chuẩn FCM HTTP v1 để kiểm tra và gửi sang Firebase Gateway
    /// </summary>
    object BuildFcmHttpV1Payload(PushNotificationRequest request);
}
