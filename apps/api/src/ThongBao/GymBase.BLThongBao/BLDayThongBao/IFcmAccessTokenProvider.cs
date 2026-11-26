namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Lấy access token OAuth2 (scope firebase.messaging) để gọi FCM HTTP v1. Tách ra để test <see cref="FcmPushSender"/> không cần Google.</summary>
public interface IFcmAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}
