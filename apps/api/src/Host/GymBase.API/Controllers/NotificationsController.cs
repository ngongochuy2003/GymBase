using GymBase.Base.Hosting;
using GymBase.BLFile.BLNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Controllers;

/// <summary>
/// Quản lý thông báo trên kênh web: /api/v1/web/notifications
/// </summary>
[Authorize]
public sealed class NotificationsController(IPushNotificationService notificationService) : BaseApiController
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] PushNotificationRequest request, CancellationToken ct)
    {
        var result = await notificationService.BLSendAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("preview-payload")]
    [AllowAnonymous]
    public IActionResult PreviewPayload([FromBody] PushNotificationRequest request)
    {
        var payload = notificationService.BLBuildFcmHttpV1Payload(request);
        return Ok(payload);
    }
}
