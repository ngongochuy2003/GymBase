using GymBase.Base.Hosting;
using GymBase.BLAuth.NguoiDung;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
builder.Services.AddAppApi(builder.Configuration);

// Một AppDbContext cho cả DB; mỗi DL.*.MySql có cấu hình entity thì thêm assembly của nó vào đây.
builder.Services.AddAppPersistence(builder.Configuration, typeof(UserRepository).Assembly);

// Auth: người dùng
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseAppApi();

app.Run();
