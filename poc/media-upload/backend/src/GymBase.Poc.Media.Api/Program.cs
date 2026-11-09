using GymBase.Poc.Media.Endpoints;
using GymBase.Poc.Media.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "GymBase V1 Storage API (Local Storage & S3 Ready)",
        Version = "v1",
        Description = "API lưu trữ file chuẩn V1: Local Storage trước, sẵn sàng chuyển S3, bảng tep_tin với nha_cung_cap."
    });
});

// 2. CORS cho Mobile App (Expo development server & emulator)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllForDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Đăng ký Hạ tầng lưu trữ chuẩn V1 + Presigned URL S3
builder.Services.AddGymBaseFileStorage(builder.Configuration);
builder.Services.AddMediaUpload(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAllForDev");

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymBase V1 Storage API");
        c.RoutePrefix = string.Empty; // Mở Swagger tại root URL
    });
}

// 4. Health Check
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    module = "GymBase.V1.Storage",
    mode = "Local Storage (S3-Ready)"
})).WithTags("Health");

// 5. Đăng ký Endpoints chuẩn V1 (/api/files) và Direct S3 (/api/v1/media)
app.MapFilesEndpoints();
app.MapMediaEndpoints();

app.Run();

// Hỗ trợ WebApplicationFactory trong Integration Tests
public partial class Program { }
