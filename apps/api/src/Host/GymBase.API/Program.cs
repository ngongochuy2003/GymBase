using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Errors;
using GymBase.BLAuth.NguoiDung;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql.Persistence;

var builder = WebApplication.CreateBuilder(args);

// TODO(HOST): gom cấu hình chung vào AddAppApi()/UseAppApi() trong GymBase.Base.Hosting:
//   CORS đọc origin từ cấu hình, LowercaseUrls, xử lý lỗi (AddAppErrorHandling), TimeProvider. Mục 11.

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
builder.Services.AddControllers(options => options.UseApiChannels());
builder.Services.AddAppErrorHandling(); // Lỗi nào cũng trả ProblemDetails (code, trace_id)
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton(TimeProvider.System);

// Một AppDbContext cho cả DB; mỗi DL.*.MySql có cấu hình entity thì thêm assembly của nó vào đây.
builder.Services.AddAppPersistence(builder.Configuration, typeof(UserRepository).Assembly);

// Auth: người dùng
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAppErrorHandling();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
