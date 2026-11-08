using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Errors;
using GymBase.BLAuth.NguoiDung;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        public static async Task<WebApplication> StartAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
            builder.WebHost.UseTestServer();
            builder.Services.AddControllers(options => options.UseApiChannels())
                .AddApplicationPart(typeof(TestApp).Assembly);
            builder.Services.AddAppErrorHandling();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton(typeof(InMemoryStore<>));
            builder.Services.AddScoped<IUserDL, UserDL>();
            builder.Services.AddScoped<UserBL>();

            var app = builder.Build();
            app.UseAppErrorHandling();
            app.MapControllers();
            await app.StartAsync();
            return app;
        }
    }
}

namespace GymBase.IntegrationTests.Controllers
{
    /// <summary>CRUD người dùng cho web: /api/v1/web/testusers.</summary>
    public sealed class TestUsersController(UserBL bl)
        : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(bl);

    /// <summary>Action ném lỗi có chủ đích: /api/v1/web/errortest/...</summary>
    public sealed class ErrorTestController : BaseApiController
    {
        [HttpGet("rule")]
        public IActionResult Rule() => throw new BusinessRuleException("Gói tập đã hết hạn.", "MEMBERSHIP_EXPIRED");

        [HttpGet("boom")]
        public IActionResult Boom() => throw new InvalidOperationException("Chuỗi kết nối bí mật");
    }
}

namespace GymBase.IntegrationTests.Mob.Controllers
{
    /// <summary>Cùng tên với controller web nhưng ở Mob/Controllers: /api/v1/mob/testusers.</summary>
    public sealed class TestUsersController(UserBL bl)
        : BaseCrudController<NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto>(bl);
}
