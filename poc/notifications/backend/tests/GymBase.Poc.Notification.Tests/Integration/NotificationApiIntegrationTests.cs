using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymBase.Poc.Notification.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GymBase.Poc.Notification.Tests.Integration;

public class NotificationApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public NotificationApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }

    [Fact]
    public async Task SendNotificationEndpoint_ShouldReturn200Ok_AndMessageId()
    {
        var client = _factory.CreateClient();

        var request = new PushNotificationRequest(
            DeviceToken: "device-test-token",
            Title: "GymBase Notification",
            Body: "Bạn có lịch tập lúc 19:00",
            ChannelId: NotificationChannels.Alarms
        );

        var response = await client.PostAsJsonAsync("/api/v1/notifications/send", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PushNotificationResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.MessageId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PreviewPayloadEndpoint_ShouldReturnValidFcmStructure()
    {
        var client = _factory.CreateClient();

        var request = new PushNotificationRequest(
            DeviceToken: "device-test-token",
            Title: "Preview Test",
            Body: "Testing payload content"
        );

        var response = await client.PostAsJsonAsync("/api/v1/notifications/preview-payload", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("priority");
        json.Should().Contain("android");
        json.Should().Contain("apns");
    }
}
