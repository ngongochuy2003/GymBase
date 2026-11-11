using System.Net.Http.Headers;
using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GymBase.IntegrationTests
{
    /// <summary>
    /// App chạy trên TestServer với cấu hình giống GymBase.API (route theo kênh, xử lý lỗi, người dùng mock)
    /// và các controller thử trong assembly test.
    /// </summary>
    internal static class TestApp
    {
        public const string AllowedOrigin = "https://admin.gymbase.test";

        /// <summary>Khóa ký chỉ dùng trong test (Base64 của 32 byte).</summary>
        public const string SigningKey = "dGVzdC1vbmx5LXNpZ25pbmcta2V5LTMyLWJ5dGVzLWxvbmc=";

        public static async Task<WebApplication> StartAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
            builder.WebHost.UseTestServer();
            builder.Configuration[$"{AppApiExtensions.CorsOriginsSection}:0"] = AllowedOrigin;
            builder.Configuration["Jwt:SigningKey"] = SigningKey;
            builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly)
                .AddApplicationPart(typeof(TestApp).Assembly)
                .AddApplicationPart(typeof(GymBase.API.Controllers.AuthController).Assembly);
            builder.Services.AddAuthBusiness();
            builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1_000);
            builder.Services.AddSingleton(typeof(InMemoryStore<>));
            builder.Services.AddSingleton<RecordingAuditLogWriter>();
            builder.Services.AddSingleton<IAuditLogWriter>(services => services.GetRequiredService<RecordingAuditLogWriter>());
            builder.Services.AddScoped<ITaiKhoanRepository, InMemoryTaiKhoanRepository>();
            builder.Services.AddScoped<IPhienDangNhapRepository, InMemoryPhienDangNhapRepository>();

            var app = builder.Build();
            app.UseAppApi();
            await app.StartAsync();
            return app;
        }

        /// <summary>Client mang access token thật do app cấp, với vai trò cho trước.</summary>
        public static HttpClient ClientWithRole(WebApplication app, string role)
        {
            var token = app.Services.GetRequiredService<ITokenIssuer>()
                .CreateAccessToken(new AccessTokenRequest(Guid.CreateVersion7(), "test.user", role, Guid.CreateVersion7()));
            var client = app.GetTestClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            return client;
        }
    }
}

namespace GymBase.IntegrationTests.Controllers
{
    /// <summary>CRUD người dùng cho web, không cần đăng nhập: /api/v1/web/testusers.</summary>
    [AllowAnonymous]
    public sealed class TestUsersController(TaiKhoanService service)
        : BaseCrudController<TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto>(service);

    /// <summary>Action ném lỗi có chủ đích: /api/v1/web/errortest/...</summary>
    [AllowAnonymous]
    public sealed class ErrorTestController : BaseApiController
    {
        [HttpGet("rule")]
        public IActionResult Rule() => throw new BusinessRuleException("Gói tập đã hết hạn.", "MEMBERSHIP_EXPIRED");

        [HttpGet("boom")]
        public IActionResult Boom() => throw new InvalidOperationException("Chuỗi kết nối bí mật");
    }

    /// <summary>Route tường minh có gạch nối, thay cho route lấy theo tên controller: /api/v1/web/buoi-tap/{id}/xac-nhan.</summary>
    [AllowAnonymous]
    [Route("buoi-tap")]
    public abstract class TestBuoiTapControllerBase : BaseApiController
    {
        [HttpPost("{id:guid}/xac-nhan")]
        public IActionResult XacNhan(Guid id) => Ok(new { id });
    }

    /// <summary>Lớp con rỗng như <c>AuthController</c>: route lấy từ lớp gốc trung gian, không phải từ tên controller.</summary>
    public sealed class TestBuoiTapController : TestBuoiTapControllerBase;
}

namespace GymBase.IntegrationTests.Mob.Controllers
{
    /// <summary>Cùng tên với controller web nhưng ở Mob/Controllers: /api/v1/mob/testusers.</summary>
    [AllowAnonymous]
    public sealed class TestUsersController(TaiKhoanService service)
        : BaseCrudController<TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto>(service);
}
