using GymBase.AMISBase.API;
var builder = WebApplication.CreateBuilder(args);

// Mọi route của host này có tiền tố /web (ví dụ /web/api/v1/users)
builder.Services.AddControllers(options => options.UseApiChannel(ApiChannel.Web));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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
