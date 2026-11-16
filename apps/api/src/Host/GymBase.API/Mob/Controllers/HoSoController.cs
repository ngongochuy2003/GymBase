using GymBase.Base.Hosting;
using GymBase.BLBiz.BLHoSo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Hồ sơ người dùng sau onboarding cho app mobile: <c>/api/v1/mob/ho-so</c>. Một lần lưu ghi cả hồ sơ và chỉ số
/// cơ thể; đợt này app chỉ gửi khối học viên.
/// </summary>
[Authorize]
[Route("ho-so")]
public sealed class HoSoController(HoSoService hoSoService) : BaseApiController
{
    /// <summary>Hồ sơ hiện tại + chỉ số cơ thể mới nhất; 404 <c>PROFILE_NOT_FOUND</c> khi chưa thiết lập.</summary>
    [HttpGet]
    public async Task<ActionResult<HoSoDto>> Lay(CancellationToken cancellationToken) =>
        Ok(await hoSoService.BLXemHoSoAsync(cancellationToken));

    /// <summary>Lưu hồ sơ và chỉ số cơ thể trong một transaction; trả hồ sơ vừa lưu.</summary>
    [HttpPut]
    public async Task<ActionResult<HoSoDto>> Luu([FromBody] HoSoLuuDto dto, CancellationToken cancellationToken) =>
        Ok(await hoSoService.BLLuuHoSoAsync(dto, cancellationToken));
}
