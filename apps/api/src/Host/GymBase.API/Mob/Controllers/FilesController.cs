using GymBase.Base.Hosting;
using GymBase.BLFile.BLFiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Quản lý file trên kênh mobile: /api/v1/mob/files
/// Yêu cầu xác thực khi tải lên (Upload); cho phép truy cập công khai khi xem nội dung (GetContent).
/// Tự động lưu trữ trên MinIO / S3 tương thích ngược với fallback tệp cục bộ.
/// </summary>
public sealed class FilesController(IFileService fileService, IWebHostEnvironment env) : BaseApiController
{
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, [FromForm] string? purpose, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Tệp tin tải lên rỗng." });
        }

        var actualPurpose = string.IsNullOrWhiteSpace(purpose) ? "post" : purpose.Trim().ToLowerInvariant();
        var result = await fileService.BLUploadAsync(User, file, actualPurpose, ct);

        return Ok(new
        {
            file_id = result.FileId,
            fileId = result.FileId,
            original_name = result.OriginalName,
            originalName = result.OriginalName,
            content_type = result.ContentType,
            contentType = result.ContentType,
            size = result.Size,
            purpose = result.Purpose,
            provider = result.Provider,
            blurhash = result.Blurhash,
            content_url = $"/api/v1/mob/files/{result.FileId}/content",
            contentUrl = $"/api/v1/mob/files/{result.FileId}/content"
        });
    }

    [HttpGet("{id:guid}/content")]
    [HttpGet("/api/files/{id:guid}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContent(Guid id, CancellationToken ct)
    {
        // 1. Kiểm tra file legacy trên ổ đĩa nếu tồn tại
        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        if (Directory.Exists(uploadsDir))
        {
            var idPrefix = id.ToString("N");
            var matchingFiles = Directory.GetFiles(uploadsDir, $"{idPrefix}*");
            if (matchingFiles.Length == 0)
            {
                matchingFiles = Directory.GetFiles(uploadsDir, $"{id}*");
            }

            if (matchingFiles.Length > 0)
            {
                var filePath = matchingFiles[0];
                var ext = Path.GetExtension(filePath);
                var mime = GetMimeType(ext);
                return PhysicalFile(filePath, mime, enableRangeProcessing: true);
            }
        }

        // 2. Mở qua IFileService (MinIO / S3 / Local chuẩn)
        try
        {
            var streamResult = await fileService.BLOpenAuthorizedAsync(User, id, ct);
            return File(streamResult.Stream, streamResult.ContentType, enableRangeProcessing: true);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private static string GetMimeType(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".mp4" => "video/mp4",
        ".mov" => "video/quicktime",
        _ => "application/octet-stream",
    };
}
