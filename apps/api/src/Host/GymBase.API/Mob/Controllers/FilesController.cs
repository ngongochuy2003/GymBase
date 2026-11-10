using GymBase.Base.Hosting;
using GymBase.BLFile.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Quản lý file trên kênh mobile: /api/v1/mob/files
/// </summary>
[Authorize]
public sealed class FilesController(IFileService fileService) : BaseApiController
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? purpose, CancellationToken ct)
    {
        var actualPurpose = purpose ?? "avatar";
        var result = await fileService.UploadAsync(User, file, actualPurpose, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
    {
        var meta = await fileService.GetMetadataAsync(User, id, ct);
        return meta != null ? Ok(meta) : NotFound();
    }

    [HttpGet("{id:guid}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContent(Guid id, CancellationToken ct)
    {
        var streamResult = await fileService.OpenAuthorizedAsync(User, id, ct);
        return File(streamResult.Stream, streamResult.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await fileService.DeleteAsync(User, id, ct);
        return NoContent();
    }
}
