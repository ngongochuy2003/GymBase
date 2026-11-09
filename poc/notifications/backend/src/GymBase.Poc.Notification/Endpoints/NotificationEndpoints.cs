using GymBase.Poc.Notification.Contracts;
using GymBase.Poc.Notification.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GymBase.Poc.Notification.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Lock Screen Notifications (Push & Alarm)");

        // 1. POST /api/v1/notifications/send
        group.MapPost("/send", async (
            [FromBody] PushNotificationRequest request,
            IPushNotificationService notificationService,
            CancellationToken ct) =>
        {
            var result = await notificationService.SendAsync(request, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("SendPushNotification")
        .WithSummary("Bắn thông báo đẩy lên thiết bị người dùng (sáng màn hình khóa, rung chuông).");

        // 2. POST /api/v1/notifications/preview-payload
        group.MapPost("/preview-payload", (
            [FromBody] PushNotificationRequest request,
            IPushNotificationService notificationService) =>
        {
            var payload = notificationService.BuildFcmHttpV1Payload(request);
            return Results.Ok(payload);
        })
        .WithName("PreviewFcmPayload")
        .WithSummary("Xem trước cấu trúc JSON FCM v1 (chứa Android priority HIGH, visibility PUBLIC, APNs time-sensitive).");

        return app;
    }
}
