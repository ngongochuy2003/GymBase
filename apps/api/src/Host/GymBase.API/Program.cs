using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Logging;
using GymBase.BLAuth;
using GymBase.BLBiz;
using GymBase.BLBiz.BaiViet;
using GymBase.BLFile;
using GymBase.BLFile.BLFiles;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.Contract;
using GymBase.DLBiz.MySql;
using GymBase.DLFile.Contract;
using GymBase.DLFile.MySql;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLAuth.BLXacThuc;
using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.DLAuth.MySql.DLDangNhapNgoai;
using GymBase.DLAuth.MySql.DLPhienDangNhap;
using GymBase.DLAuth.MySql.DLTaiKhoan;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.DLBiz.MySql.DLHoSoNguoiDung;
using GymBase.DLBiz.MySql.DLKetNoiPt;
using GymBase.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
// Mỗi BL có validator thì thêm assembly của nó vào đây.
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly, typeof(BaiVietService).Assembly);

// Log kỹ thuật ra console và file, cấu hình ở mục "Serilog" của appsettings.
builder.Services.AddAppLogging(builder.Configuration);

// Một AppDbContext cho cả DB.
builder.Services.AddAppPersistence(builder.Configuration);

// Auth: người dùng, đăng nhập
builder.Services.AddAuthBusiness();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<IPhienDangNhapRepository, PhienDangNhapRepository>();
builder.Services.AddScoped<IDangNhapNgoaiRepository, DangNhapNgoaiRepository>();

// BIZ: mạng xã hội (bài viết, tương tác thích/bình luận)
builder.Services.AddBizBusiness();
builder.Services.AddScoped<IBaiVietRepository, BaiVietRepository>();
builder.Services.AddScoped<ITuongTacBaiVietRepository, TuongTacBaiVietRepository>();

// File: lưu trữ tệp tin (Local/S3) và push notification
builder.Services.AddFileBusiness(builder.Configuration);
builder.Services.AddScoped<ITepTinRepository, TepTinRepository>();
// Biz: quyền theo hồ sơ và quyền trên dữ liệu học viên (phân quyền: docs/api/phan-quyen.md)
builder.Services.AddBizBusiness();
builder.Services.AddScoped<IHoSoNguoiDungRepository, HoSoNguoiDungRepository>();
builder.Services.AddScoped<IKetNoiPtRepository, KetNoiPtRepository>();

var app = builder.Build();

// Tạo tài khoản Admin đầu tiên: dotnet run --project src/Host/GymBase.API -- create-admin <email>
// Mật khẩu lấy từ biến môi trường để không nằm trong lịch sử dòng lệnh.
if (args is ["create-admin", var adminEmail])
{
    var password = Environment.GetEnvironmentVariable("GYMBASE_ADMIN_PASSWORD");
    if (string.IsNullOrEmpty(password))
    {
        Console.Error.WriteLine("Chưa đặt biến môi trường GYMBASE_ADMIN_PASSWORD.");
        return 1;
    }

    await using var scope = app.Services.CreateAsyncScope();
    try
    {
        await scope.ServiceProvider.GetRequiredService<XacThucService>().BLTaoHoacDatLaiAdminAsync(adminEmail, password, CancellationToken.None);
    }
    catch (ValidationFailedException ex)
    {
        Console.Error.WriteLine(string.Join(Environment.NewLine, ex.Errors!.Values.SelectMany(messages => messages)));
        return 1;
    }

    Console.WriteLine($"Đã tạo hoặc đặt lại tài khoản Admin '{adminEmail}'.");
    return 0;
}

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
