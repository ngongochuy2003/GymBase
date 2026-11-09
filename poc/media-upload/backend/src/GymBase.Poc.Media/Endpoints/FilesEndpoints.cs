using GymBase.Poc.Media.BL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GymBase.Poc.Media.Endpoints;

public static class FilesEndpoints
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files")
            .WithTags("Files (Chuẩn V1 - Local Storage & S3 Ready)");

        // 1. POST /api/files (multipart/form-data)
        group.MapPost("/", async (
            HttpContext context,
            [FromForm] string? purpose,
            IFileService fileService,
            CancellationToken ct) =>
        {
            if (!context.Request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Yêu cầu phải có định dạng multipart/form-data." });
            }

            var form = await context.Request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();

            if (file == null)
            {
                return Results.BadRequest(new { error = "Không tìm thấy tệp tin trong form field 'file'." });
            }

            var actualPurpose = purpose ?? form["purpose"].FirstOrDefault() ?? "post";

            try
            {
                var result = await fileService.UploadAsync(context.User, file, actualPurpose, ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .DisableAntiforgery()
        .WithName("UploadFile")
        .WithSummary("Upload file qua backend (.NET Core stream thẳng xuống Local Storage hoặc S3).")
        .Produces<FileUploadResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // 2. GET /api/files/{id} (Metadata)
        group.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext context,
            IFileService fileService,
            CancellationToken ct) =>
        {
            var meta = await fileService.GetMetadataAsync(context.User, id, ct);
            return meta != null ? Results.Ok(meta) : Results.NotFound(new { error = $"Tệp tin {id} không tồn tại." });
        })
        .WithName("GetFileMetadata")
        .WithSummary("Lấy thông tin metadata của tệp tin từ bảng tep_tin.")
        .Produces<TepTinMetadataDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // 3. GET /api/files/{id}/content (Stream file nhị phân có kiểm quyền)
        group.MapGet("/{id:guid}/content", async (
            Guid id,
            HttpContext context,
            IFileService fileService,
            CancellationToken ct) =>
        {
            try
            {
                var response = await fileService.OpenAuthorizedAsync(context.User, id, ct);
                return Results.File(
                    fileStream: response.Stream,
                    contentType: response.ContentType,
                    fileDownloadName: response.OriginalName,
                    enableRangeProcessing: true);
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound(new { error = $"Tệp tin {id} không tồn tại trên storage." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
        })
        .WithName("GetFileContent")
        .WithSummary("Tải hoặc stream nội dung tệp tin nhị phân sau khi kiểm tra quyền.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        // 4. DELETE /api/files/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            HttpContext context,
            IFileService fileService,
            CancellationToken ct) =>
        {
            try
            {
                await fileService.DeleteAsync(context.User, id, ct);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
        })
        .WithName("DeleteFile")
        .WithSummary("Xóa tệp tin khỏi hệ thống và storage theo quyền sở hữu.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden);

        // 5. POST /api/files/migrate-to-s3 (Migration job an toàn không downtime)
        group.MapPost("/migrate-to-s3", async (
            [FromQuery] int? batchSize,
            IFileMigrationService migrationService,
            CancellationToken ct) =>
        {
            var result = await migrationService.MigrateLocalToS3Async(batchSize ?? 50, ct);
            return Results.Ok(result);
        })
        .WithName("MigrateLocalToS3")
        .WithSummary("Chạy migration job chuyển các file từ LOCAL sang S3 không downtime.")
        .Produces<MigrationResultDto>(StatusCodes.Status200OK);

        return app;
    }
}
