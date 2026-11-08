using GymBase.AMISBase.API;
using GymBase.AMISBase.API.Errors;

var builder = WebApplication.CreateBuilder(args);

// Mọi route của host này có tiền tố /api/web (ví dụ /api/web/v1/users)
builder.Services.AddControllers(options => options.UseApiChannel(ApiChannel.Web));
builder.Services.AddAppErrorHandling(); // Lỗi nào cũng trả ProblemDetails (code, trace_id)
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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
