using GymBase.Poc.Media.Endpoints;
using GymBase.Poc.Media.Extensions;
using GymBase.Poc.Media.Storage;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "GymBase Media Upload POC API",
        Version = "v1",
        Description = "POC kiến trúc upload ảnh trực tiếp S3 với Presigned URL, biến thể WebP, Blurhash và Deduplication."
    });
});

// 2. Cấu hình CORS cho Mobile App (Expo development server & emulator)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllForDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Đăng ký Media Upload Module
builder.Services.AddMediaUpload(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAllForDev");

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymBase Media API v1");
        c.RoutePrefix = string.Empty; // Mở Swagger ngay tại root URL
    });
}

// 4. Endpoint kiểm tra sức khỏe
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    module = "GymBase.Poc.Media"
})).WithTags("Health");

// 5. Đăng ký toàn bộ Media Endpoints
app.MapMediaEndpoints();

// 6. Endpoint Mock PUT cho InMemory Storage (cho phép test upload trực tiếp mà không cần khởi động MinIO)
app.MapPut("/mock-s3-upload/{*key}", async (string key, HttpContext context, IMediaStorage storage) =>
{
    if (storage is InMemoryMediaStorage inMemory)
    {
        using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var contentType = context.Request.ContentType ?? "image/jpeg";
        inMemory.DirectPut(key, bytes, contentType);
        return Results.Ok(new { message = "Mock S3 PUT uploaded successfully", key, size = bytes.Length });
    }
    return Results.BadRequest(new { error = "Not running in InMemory storage mode." });
}).ExcludeFromDescription();

app.Run();

// Hỗ trợ WebApplicationFactory trong Integration Tests
public partial class Program { }
