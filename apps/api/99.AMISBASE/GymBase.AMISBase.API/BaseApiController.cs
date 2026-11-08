using Microsoft.AspNetCore.Mvc;

namespace GymBase.AMISBase.API;

/// <summary>
/// Controller gốc cho mọi API (DIAPI, BIZAPI, MobAPI).
/// Controller chỉ nhận request và gọi BL; không chứa logic nghiệp vụ, không truy cập DB.
/// Controller chỉ khai báo v1/[controller]; route thực tế có thêm tiền tố kênh: /api/web/v1/... hoặc /api/mobile/v1/... (xem <see cref="RoutePrefixConvention"/>).
/// </summary>
[ApiController]
[Route("v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
