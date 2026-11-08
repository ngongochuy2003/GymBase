using GymBase.AMISBase.API;
using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.BLSystem.Users;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.MobAPI.Controllers;

/// <summary>
/// API người dùng cho app mobile. Route: /api/mobile/v1/users (tiền tố /api/mobile do Program.cs gắn).
/// Chỉ nhận request và gọi <see cref="UserBL"/>; logic dùng chung với
/// SystemAPI nằm trong BL.
/// </summary>
public sealed class UsersController(UserBL userBL) : BaseApiController
{
    /// <summary>Danh sách người dùng có phân trang. Ví dụ: GET /api/mobile/v1/users?page=1&amp;pageSize=20&amp;search=pt</summary>
    [HttpGet]
    public async Task<ActionResult<PagingResult<UserDto>>> GetPaging(
        [FromQuery] PagingRequest request, CancellationToken cancellationToken) =>
        Ok(await userBL.GetPagingAsync(request, cancellationToken));

    /// <summary>Chi tiết một người dùng; 404 nếu không tồn tại.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userBL.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
