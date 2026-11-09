using FluentAssertions;
using GymBase.Poc.Notification.Configuration;
using GymBase.Poc.Notification.Contracts;
using GymBase.Poc.Notification.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.Poc.Notification.Tests.Unit;

public class FcmPayloadBuilderTests
{
    private readonly FcmV1PushNotificationService _service;

    public FcmPayloadBuilderTests()
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
    public void BuildFcmHttpV1Payload_ForUrgentMessage_ShouldIncludeHighPriorityAndLockScreenVisibility()
    {
        // Arrange
        var request = new PushNotificationRequest(
            DeviceToken: "sample-fcm-token-12345",
            Title: "HLV Tuấn Anh",
            Body: "Lịch tập hôm nay dời lên 17:30 nhé!",
            ChannelId: NotificationChannels.UrgentMessages,
            Priority: PushPriority.High,
            AvatarUrl: "https://gymbase.vn/avatars/coach.jpg"
        );

        // Act
        var payloadObj = _service.BuildFcmHttpV1Payload(request);
        var json = System.Text.Json.JsonSerializer.Serialize(payloadObj);

        // Assert
        json.Should().Contain("\"priority\":\"HIGH\"", "Bắt buộc HIGH để đánh thức Doze mode");
        json.Should().Contain("\"channel_id\":\"gymbase_urgent_messages\"");
        json.Should().Contain("\"notification_priority\":\"PRIORITY_MAX\"", "Bắt buộc PRIORITY_MAX để hiện heads-up banner");
        json.Should().Contain("\"visibility\":\"PUBLIC\"", "Bắt buộc PUBLIC để hiển thị trên màn hình khóa");
        json.Should().Contain("\"interruption-level\":\"time-sensitive\"", "Bắt buộc time-sensitive trên iOS để vượt chế độ Focus");
        json.Should().Contain("\"apns-priority\":\"10\"");
    }

    [Fact]
    public async Task SendAsync_EmptyToken_ShouldReturnFailure()
    {
        var request = new PushNotificationRequest(
            DeviceToken: "",
            Title: "Test",
            Body: "Content"
        );

        var result = await _service.SendAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("không được để trống");
    }

    [Fact]
    public async Task SendAsync_ValidRequest_InSimulateMode_ShouldReturnSuccess()
    {
        var request = new PushNotificationRequest(
            DeviceToken: "token-abc-xyz",
            Title: "Nhắc tập",
            Body: "Đến giờ tập chân rồi!"
        );

        var result = await _service.SendAsync(request);

        result.Success.Should().BeTrue();
        result.MessageId.Should().StartWith("simulated-fcm-");
        result.FcmPayload.Should().NotBeNull();
    }
}
