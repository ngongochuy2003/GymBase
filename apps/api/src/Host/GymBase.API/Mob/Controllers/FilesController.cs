using GymBase.Base.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Quản lý file trên kênh mobile: /api/v1/mob/files
/// Cho phép truy cập từ cả phiên có JWT và phiên trải nghiệm trên ứng dụng di động.
/// </summary>
[AllowAnonymous]
public sealed class FilesController(IWebHostEnvironment env) : BaseApiController
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov"
    };

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, [FromForm] string? purpose, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Tệp tin tải lên rỗng." });
        }

        if (file.Length > 15 * 1024 * 1024)
        {
            return BadRequest(new { message = "Dung lượng tệp tin vượt quá giới hạn 15MB." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            ext = ".jpg";
        }

        var fileId = Guid.NewGuid();
        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var savedFileName = $"{fileId:N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadsDir, savedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var actualPurpose = string.IsNullOrWhiteSpace(purpose) ? "post" : purpose.Trim().ToLowerInvariant();
        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType) || contentType == "application/octet-stream")
        {
            contentType = GetMimeType(ext);
        }

        return Ok(new
        {
            file_id = fileId,
            fileId,
            original_name = Path.GetFileName(file.FileName),
            originalName = Path.GetFileName(file.FileName),
            content_type = contentType,
            contentType,
            size = file.Length,
            purpose = actualPurpose,
            provider = "LOCAL",
            content_url = $"/api/v1/mob/files/{fileId}/content",
            contentUrl = $"/api/v1/mob/files/{fileId}/content"
        });
    }

    [HttpGet("{id:guid}/content")]
    public IActionResult GetContent(Guid id)
    {
        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            return NotFound();
        }

        var idPrefix = id.ToString("N");
        var matchingFiles = Directory.GetFiles(uploadsDir, $"{idPrefix}*");
        if (matchingFiles.Length == 0)
        {
            // Thử định dạng có gạch nối
            matchingFiles = Directory.GetFiles(uploadsDir, $"{id}*");
        }

        if (matchingFiles.Length == 0)
        {
            return NotFound();
        }

        var filePath = matchingFiles[0];
        var ext = Path.GetExtension(filePath);
        var mime = GetMimeType(ext);

        return PhysicalFile(filePath, mime, enableRangeProcessing: true);
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
