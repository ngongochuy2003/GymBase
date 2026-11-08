using GymBase.AMISBase.API;
using GymBase.BLSystem.Users;
using GymBase.DLSystem.Contract;
using GymBase.DLSystem.Mysql;

var builder = WebApplication.CreateBuilder(args);

// Mọi route của host này có tiền tố /api/web (ví dụ /api/web/v1/users)
builder.Services.AddControllers(options => options.UseApiChannel(ApiChannel.Web));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// System: người dùng (UserDL hiện là MOCK, chưa nối DB)
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
