using GymBase.AMISBase.API;
using GymBase.AMISBase.DL.Mysql.InMemory;
using GymBase.BLSystem.NguoiDung;
using GymBase.DLSystem.Contract;
using GymBase.DLSystem.Mysql;

var builder = WebApplication.CreateBuilder(args);

// TODO(HOST): gom cấu hình chung của 8 host vào AddGymBaseApi()/UseGymBaseApi() trong AMISBase.API:
//   CORS đọc origin từ cấu hình, LowercaseUrls, exception handler, TimeProvider. Mục 11.

// Mọi route của host này có tiền tố /api/web (ví dụ /api/web/v1/users)
builder.Services.AddControllers(options => options.UseApiChannel(ApiChannel.Web));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// System: người dùng (UserDL hiện là MOCK, chưa nối DB)
builder.Services.AddSingleton(TimeProvider.System);
// TODO(DB): bỏ InMemoryStore khi nối DB; đăng ký DbContext (EF Core), thêm health check DbContext. Mục 3, 16.
builder.Services.AddSingleton(typeof(InMemoryStore<>)); // MOCK: "bảng" trong bộ nhớ, bỏ khi nối DB
builder.Services.AddScoped<IUserDL, UserDL>();
builder.Services.AddScoped<UserBL>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
