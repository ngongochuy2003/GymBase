using System.Security.Claims;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.BLFile.BLFiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// File trên kênh mobile: <c>/api/v1/mob/files</c>. App xin form upload, gửi thẳng lên MinIO/S3, báo xong, rồi xem file bằng URL đã ký;
/// byte của file không đi qua API. Xem docs/architecture/upload-file.md.
/// </summary>
[Authorize]
public sealed class FilesController(ITepTinService tepTinService) : BaseApiController
{
    private Guid NguoiDungId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedException();

    /// <summary>Bước 1 của upload: xin form upload đã ký (presigned POST).</summary>
    [HttpPost("upload-url")]
    public async Task<IActionResult> XinUpload([FromBody] XinUploadRequestDto request, CancellationToken ct) =>
        Ok(await tepTinService.BLXinUploadAsync(NguoiDungId, request, ct));

    /// <summary>Bước 3 của upload: báo đã gửi xong, server kiểm object trên storage.</summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> XacNhanUpload(Guid id, CancellationToken ct) =>
        Ok(await tepTinService.BLXacNhanUploadAsync(NguoiDungId, id, ct));

    /// <summary>URL đọc (presigned GET) của một file, sau khi kiểm quyền.</summary>
    [HttpGet("{id:guid}/url")]
    public async Task<IActionResult> LayUrl(Guid id, CancellationToken ct) =>
        Ok(await tepTinService.BLLayUrlAsync(NguoiDungId, id, ct));

    /// <summary>URL đọc của nhiều file một lần; file không được xem thì vắng mặt trong <c>data</c>.</summary>
    [HttpPost("urls")]
    public async Task<IActionResult> LayUrls([FromBody] LayUrlsRequestDto request, CancellationToken ct)
    {
        var urls = await tepTinService.BLLayUrlsAsync(NguoiDungId, request.FileIds ?? [], ct);
        return Ok(new { data = urls.Values });
    }

    /// <summary>Xóa file đã upload mà chưa gắn vào đâu (ví dụ người dùng bỏ ảnh đã chọn).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Xoa(Guid id, CancellationToken ct)
    {
        await tepTinService.BLXoaAsync(NguoiDungId, id, ct);
        return NoContent();
    }
}
