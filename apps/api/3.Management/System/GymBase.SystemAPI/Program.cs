using GymBase.BLSystem.Users;
using GymBase.DLSystem.Contract;
using GymBase.DLSystem.Mysql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
