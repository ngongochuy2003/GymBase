using GymBase.BLFile.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.File;

public class NotificationServiceTests
{
    private readonly FcmV1PushNotificationService _service;

    public NotificationServiceTests()
    {
        var options = new NotificationOptions
        {
            FirebaseProjectId = "test-project",
            SimulateMode = true
        };

        _service = new FcmV1PushNotificationService(
            Options.Create(options),
            NullLogger<FcmV1PushNotificationService>.Instance);
    }

    [Fact]
    public void BuildFcmHttpV1Payload_ShouldIncludeHighPriorityAndLockScreenVisibility()
    {
        var request = new PushNotificationRequest(
            DeviceToken: "token-test-12345",
            Title: "HLV Tuấn Anh",
            Body: "18h hôm nay tập lưng nhé!",
            ChannelId: NotificationChannels.UrgentMessages,
            Priority: PushPriority.High,
            AvatarUrl: "https://gymbase.vn/avatar.jpg"
        );

        var payload = _service.BuildFcmHttpV1Payload(request);
        var json = System.Text.Json.JsonSerializer.Serialize(payload);

        Assert.Contains("\"priority\":\"HIGH\"", json);
        Assert.Contains("\"channel_id\":\"gymbase_urgent_messages\"", json);
        Assert.Contains("\"notification_priority\":\"PRIORITY_MAX\"", json);
        Assert.Contains("\"visibility\":\"PUBLIC\"", json);
        Assert.Contains("\"interruption-level\":\"time-sensitive\"", json);
        Assert.Contains("\"apns-priority\":\"10\"", json);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_SimulateMode_ShouldReturnSuccess()
    {
        var request = new PushNotificationRequest(
            DeviceToken: "valid-device-token",
            Title: "Báo thức nhắc tập",
            Body: "Tập gym sau 30 phút",
            ChannelId: NotificationChannels.Alarms
        );

        var result = await _service.SendAsync(request);

        Assert.True(result.Success);
        Assert.StartsWith("simulated-fcm-", result.MessageId);
        Assert.NotNull(result.FcmPayload);
    }

    [Fact]
    public async Task SendAsync_EmptyToken_ShouldReturnFailure()
    {
        var request = new PushNotificationRequest(
            DeviceToken: "",
            Title: "Test",
            Body: "No token"
        );

        var result = await _service.SendAsync(request);

        Assert.False(result.Success);
        Assert.Contains("không được để trống", result.ErrorMessage);
    }
}
