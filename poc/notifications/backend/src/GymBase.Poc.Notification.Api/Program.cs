using GymBase.Poc.Notification.Endpoints;
using GymBase.Poc.Notification.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "GymBase Notification API (Lock Screen Push & Alarm)",
        Version = "v1",
        Description = "API bắn thông báo lên màn hình khóa và báo thức cho PT & Học viên GymBase."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllForDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Đăng ký dịch vụ Notification
builder.Services.AddGymBaseNotifications(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAllForDev");

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymBase Notification API");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    module = "GymBase.Poc.Notification"
})).WithTags("Health");

app.MapNotificationEndpoints();

app.Run();

public partial class Program { }
