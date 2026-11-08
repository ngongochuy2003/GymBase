using Microsoft.AspNetCore.Mvc;

namespace GymBase.Base.Hosting;

/// <summary>
/// Controller gốc cho mọi API, cả web (<c>Controllers/</c>) lẫn mobile (<c>Mob/Controllers/</c>).
/// Controller chỉ nhận request, kiểm tra model/quyền, gọi BL rồi trả HTTP response; không chứa logic
/// nghiệp vụ, không truy cập DB. Controller chỉ khai báo <c>[controller]</c>; route thực tế có thêm
/// tiền tố kênh <c>/api/v1/web/...</c> hoặc <c>/api/v1/mob/...</c> (xem <see cref="ChannelRouteConvention"/>).
/// </summary>
[ApiController]
[Route("[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
