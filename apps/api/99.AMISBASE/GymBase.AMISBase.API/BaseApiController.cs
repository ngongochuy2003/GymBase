using Microsoft.AspNetCore.Mvc;

namespace GymBase.AMISBase.API;

/// <summary>
/// Controller gốc cho mọi API (DIAPI, BIZAPI, MobAPI).
/// Controller chỉ nhận request và gọi BL; không chứa logic nghiệp vụ, không truy cập DB.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
