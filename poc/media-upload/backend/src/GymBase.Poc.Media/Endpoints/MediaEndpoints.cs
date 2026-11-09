using System.Security.Claims;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/media")
            .WithTags("Media Upload");

        group.MapPost("/upload-url", async (
            [FromBody] UploadUrlRequest request,
            HttpContext httpContext,
            IMediaService mediaService,
            IOptions<MediaOptions> options,
            CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext, options.Value);
            if (ownerId == null && !options.Value.AllowAnonymousForPoc)
            {
                return Results.Unauthorized();
            }

            try
            {
                var response = await mediaService.GetUploadUrlAsync(request, ownerId, ct);
                return Results.Ok(response);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetMediaUploadUrl")
        .WithSummary("Xin cấp Presigned URL để Mobile upload trực tiếp lên S3/MinIO.")
        .Produces<UploadUrlResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/confirm", async (
            [FromBody] ConfirmUploadRequest request,
            HttpContext httpContext,
            IMediaService mediaService,
            IOptions<MediaOptions> options,
            CancellationToken ct) =>
        {
            var ownerId = GetOwnerId(httpContext, options.Value);
            if (ownerId == null && !options.Value.AllowAnonymousForPoc)
            {
                return Results.Unauthorized();
            }

            try
            {
                var response = await mediaService.ConfirmUploadAsync(request, ownerId, ct);
                return Results.Ok(response);
            }
            catch (FileNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("ConfirmMediaUpload")
        .WithSummary("Xác nhận upload sau khi Mobile đã PUT dữ liệu lên storage.")
        .Produces<ConfirmUploadResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id}", async (
            string id,
            IMediaService mediaService,
            CancellationToken ct) =>
        {
            var asset = await mediaService.GetAssetByIdAsync(id, ct);
            return asset is not null ? Results.Ok(asset) : Results.NotFound();
        })
        .WithName("GetMediaAssetById")
        .WithSummary("Lấy thông tin metadata của ảnh theo MediaId.")
        .Produces<MediaAsset>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static string? GetOwnerId(HttpContext context, MediaOptions options)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(claim))
        {
            return claim;
        }

        return options.AllowAnonymousForPoc ? "poc-anonymous-user" : null;
    }
}
