using GymBase.Base.Hosting;
using GymBase.BLFile.BLFiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Controllers;

/// <summary>
/// Quản lý file trên kênh web: /api/v1/web/files
/// </summary>
[Authorize]
public sealed class FilesController(IFileService fileService, IFileMigrationService migrationService) : BaseApiController
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? purpose, CancellationToken ct)
    {
        var actualPurpose = purpose ?? "post";
        var result = await fileService.BLUploadAsync(User, file, actualPurpose, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
    {
        var meta = await fileService.BLGetMetadataAsync(User, id, ct);
        return meta != null ? Ok(meta) : NotFound();
    }

    [HttpGet("{id:guid}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContent(Guid id, CancellationToken ct)
    {
        var streamResult = await fileService.BLOpenAuthorizedAsync(User, id, ct);
        return File(streamResult.Stream, streamResult.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await fileService.BLDeleteAsync(User, id, ct);
        return NoContent();
    }

    [HttpPost("migrate-to-s3")]
    public async Task<IActionResult> MigrateToS3([FromQuery] int batchSize = 50, CancellationToken ct = default)
    {
        var result = await migrationService.BLMigrateLocalToS3Async(batchSize, ct);
        return Ok(result);
    }
}
