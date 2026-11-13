using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.TaiKhoan;
using GymBase.BLAuth.XacThuc;
using GymBase.BLBiz;
using GymBase.BLBiz.BaiViet;
using GymBase.BLFile;
using GymBase.BLFile.Files;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.Contract;
using GymBase.DLBiz.MySql;
using GymBase.DLFile.Contract;
using GymBase.DLFile.MySql;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLAuth.BLXacThuc;
using GymBase.BLBiz;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
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
builder.Services.AddAppApi(builder.Configuration, typeof(UserService).Assembly, typeof(BaiVietService).Assembly);
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly);

// Log kỹ thuật ra console và file, cấu hình ở mục "Serilog" của appsettings.
builder.Services.AddAppLogging(builder.Configuration);

// Một AppDbContext cho cả DB.
builder.Services.AddAppPersistence(builder.Configuration);

// Auth: người dùng, đăng nhập
builder.Services.AddAuthBusiness();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<IPhienDangNhapRepository, PhienDangNhapRepository>();

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

// Tạo tài khoản Admin đầu tiên: dotnet run --project src/Host/GymBase.API -- create-admin <tên>
// Mật khẩu lấy từ biến môi trường để không nằm trong lịch sử dòng lệnh.
if (args is ["create-admin", var adminUserName])
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
        await scope.ServiceProvider.GetRequiredService<XacThucService>().BLTaoHoacDatLaiAdminAsync(adminUserName, password, CancellationToken.None);
    }
    catch (ValidationFailedException ex)
    {
        Console.Error.WriteLine(string.Join(Environment.NewLine, ex.Errors!.Values.SelectMany(messages => messages)));
        return 1;
    }

    Console.WriteLine($"Đã tạo hoặc đặt lại tài khoản Admin '{adminUserName}'.");
    return 0;
}

app.UseAppApi();

// Endpoint tải nội dung file công khai dùng chung cho mobile và web (/api/files/{id}/content)
app.MapGet("/api/files/{id:guid}/content", async (Guid id, IFileService fileService, HttpContext ctx, CancellationToken ct) =>
{
    var streamResult = await fileService.OpenAuthorizedAsync(ctx.User, id, ct);
    return Results.File(streamResult.Stream, streamResult.ContentType, enableRangeProcessing: true);
}).AllowAnonymous();

app.Run();
return 0;
