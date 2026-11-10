using GymBase.Base.Hosting;
using GymBase.BLFile.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Quản lý thông báo trên kênh mobile: /api/v1/mob/notifications
/// </summary>
[Authorize]
public sealed class NotificationsController(IPushNotificationService notificationService) : BaseApiController
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] PushNotificationRequest request, CancellationToken ct)
    {
        var result = await notificationService.SendAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("preview-payload")]
    [AllowAnonymous]
    public IActionResult PreviewPayload([FromBody] PushNotificationRequest request)
    {
        var payload = notificationService.BuildFcmHttpV1Payload(request);
        return Ok(payload);
    }
}
