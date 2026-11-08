using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Errors;
using GymBase.BLAuth.NguoiDung;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql.InMemory;

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

// Auth: người dùng (UserDL hiện là MOCK, chưa nối DB)
// TODO(DB): bỏ InMemoryStore khi nối DB; đăng ký AppDbContext (EF Core), thêm health check DbContext. Mục 3, 16.
builder.Services.AddSingleton(typeof(InMemoryStore<>)); // MOCK: "bảng" trong bộ nhớ, bỏ khi nối DB
builder.Services.AddScoped<IUserDL, UserDL>();
builder.Services.AddScoped<UserBL>();

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
