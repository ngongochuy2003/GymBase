using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLAuth.BLXacThuc;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.DLAuth.MySql.DLPhienDangNhap;
using GymBase.DLAuth.MySql.DLTaiKhoan;
using GymBase.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
// Mỗi BL có validator thì thêm assembly của nó vào đây.
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly);

// Log kỹ thuật ra console và file, cấu hình ở mục "Serilog" của appsettings.
builder.Services.AddAppLogging(builder.Configuration);

// Một AppDbContext cho cả DB.
builder.Services.AddAppPersistence(builder.Configuration);

// Auth: người dùng, đăng nhập
builder.Services.AddAuthBusiness();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<IPhienDangNhapRepository, PhienDangNhapRepository>();

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
        await scope.ServiceProvider.GetRequiredService<XacThucService>().BLCreateOrResetAdminAsync(adminUserName, password, CancellationToken.None);
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

app.Run();
return 0;
