using GymBase.API;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLBiz;
using GymBase.BLBiz.BaiViet;
using GymBase.BLDI;
using GymBase.BLFile;
using GymBase.BLFile.BLFiles;
using Microsoft.AspNetCore.Authorization;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql;
using GymBase.DLDI.MySql;
using GymBase.DLFile.MySql;
using GymBase.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
// Mỗi BL có validator thì thêm assembly của nó vào đây.
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly, typeof(BaiVietService).Assembly);

// Policy RequireProfile không đạt thì trả mã PROFILE_REQUIRED (mã của nhóm Biz nên đặt ở host).
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProfileRequiredResultHandler>();

// Log kỹ thuật ra console và file, cấu hình ở mục "Serilog" của appsettings.
builder.Services.AddAppLogging(builder.Configuration);

// Một AppDbContext cho cả DB.
builder.Services.AddAppPersistence(builder.Configuration);

// Mỗi nhóm đăng ký hai phần: nghiệp vụ (AddXxxBusiness, ở BL) và repository MySQL (AddXxxData, ở DL.MySql).
// BL không tham chiếu DL.MySql nên hai phần phải gọi riêng ở host.

// Auth: người dùng, đăng nhập
builder.Services.AddAuthBusiness();
builder.Services.AddAuthData();

// Biz: mạng xã hội (bài viết, tương tác thích/bình luận), hồ sơ, quyền theo hồ sơ và quyền trên dữ liệu học viên
// (phân quyền: docs/api/phan-quyen.md)
builder.Services.AddBizBusiness();
builder.Services.AddBizData();

// File: lưu trữ tệp tin (Local/S3) và push notification
builder.Services.AddFileBusiness(builder.Configuration);
builder.Services.AddFileData();

// DI (Danh mục): dữ liệu tham chiếu cho dropdown (docs/api/danh-muc.md)
builder.Services.AddDIBusiness();
builder.Services.AddDIData();

var app = builder.Build();

app.UseAppApi();

// Endpoint tải nội dung file công khai dùng chung cho mobile và web (/api/files/{id}/content)
app.MapGet("/api/files/{id:guid}/content", async (Guid id, IFileService fileService, IWebHostEnvironment env, HttpContext ctx, CancellationToken ct) =>
{
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
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var mime = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                _ => "application/octet-stream",
            };
            return Results.File(filePath, mime, enableRangeProcessing: true);
        }
    }

    try
    {
        var streamResult = await fileService.BLOpenAuthorizedAsync(ctx.User, id, ct);
        return Results.File(streamResult.Stream, streamResult.ContentType, enableRangeProcessing: true);
    }
    catch (Exception)
    {
        return Results.NotFound();
    }
}).AllowAnonymous();

app.Run();
return 0;
