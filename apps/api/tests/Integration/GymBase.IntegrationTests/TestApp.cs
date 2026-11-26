using System.Net.Http.Headers;
using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.Base.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLBiz;
using GymBase.BLBiz.BaiViet;
using GymBase.DLBiz.Contract;
using GymBase.BLBiz.BLTruyCap;
using GymBase.BLDI;
using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.ModuleContracts.TepTin;
using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.Entities.Auth;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            builder.Logging.ClearProviders();
            builder.WebHost.UseTestServer();
            builder.Configuration[$"{AppApiExtensions.CorsOriginsSection}:0"] = AllowedOrigin;
            builder.Configuration["Jwt:SigningKey"] = SigningKey;
            builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly, typeof(BaiVietService).Assembly)
                .AddApplicationPart(typeof(TestApp).Assembly)
                .AddApplicationPart(typeof(GymBase.API.Controllers.AuthController).Assembly);
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, GymBase.API.ProfileRequiredResultHandler>();
            builder.Services.AddAuthBusiness();
            builder.Services.AddBizBusiness();
            builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1_000);
            builder.Services.AddSingleton(typeof(InMemoryStore<>));
            builder.Services.AddSingleton<RecordingAuditLogWriter>();
            builder.Services.AddSingleton<IAuditLogWriter>(services => services.GetRequiredService<RecordingAuditLogWriter>());
            builder.Services.AddScoped<ITaiKhoanRepository, InMemoryTaiKhoanRepository>();
            builder.Services.AddScoped<IPhienDangNhapRepository, InMemoryPhienDangNhapRepository>();
            // Chứng chỉ PT: dùng ChungChiPtService thật trên tep_tin trong bộ nhớ và kho lưu trữ giả.
            builder.Services.AddScoped<ITepTinRepository, InMemoryTepTinRepository>();
            builder.Services.AddSingleton<FakeFileStorage>();
            builder.Services.AddSingleton<IStorageResolver>(services => new FakeStorageResolver(services.GetRequiredService<FakeFileStorage>()));
            builder.Services.AddScoped<IChungChiPtLienKet, ChungChiPtService>();
            // Upload hai bước (docs/architecture/upload-file.md) và controller file của mobile, trên kho giả.
            builder.Services.Configure<StorageOptions>(_ => { });
            builder.Services.AddSingleton<IImageProcessor, SkiaImageProcessor>();
            builder.Services.AddScoped<ITepTinService, TepTinService>();
            builder.Services.AddScoped<IFileLienKet>(sp => sp.GetRequiredService<ITepTinService>());
            builder.Services.AddScoped<IFileUrlProvider>(sp => sp.GetRequiredService<ITepTinService>());
            builder.Services.AddScoped<IBaiVietRepository, InMemoryBaiVietRepository>();
            builder.Services.AddScoped<ITuongTacBaiVietRepository, InMemoryTuongTacBaiVietRepository>();

            builder.Services.AddScoped<IDangNhapNgoaiRepository, InMemoryDangNhapNgoaiRepository>();
            builder.Services.AddSingleton<FakeGoogleIdTokenVerifier>();
            builder.Services.AddSingleton<IGoogleIdTokenVerifier>(services => services.GetRequiredService<FakeGoogleIdTokenVerifier>());
            builder.Services.AddBizBusiness();
            builder.Services.AddSingleton<InMemoryHoSoNguoiDungStore>();
            builder.Services.AddSingleton<InMemoryChiSoCoTheStore>();
            builder.Services.AddScoped<IHoSoNguoiDungRepository, InMemoryHoSoNguoiDungRepository>();
            builder.Services.AddScoped<IChiSoCoTheRepository, InMemoryChiSoCoTheRepository>();
            builder.Services.AddScoped<IKetNoiPtRepository, InMemoryKetNoiPtRepository>();
            
            builder.Services.AddDIBusiness();
            builder.Services.AddScoped<IDanhMucRepository, InMemoryDanhMucRepository>();

            var app = builder.Build();
            app.UseAppApi();
            await app.StartAsync();
            return app;
        }

        /// <summary>
        /// Access token chỉ dùng được khi phiên của nó còn hiệu lực và tài khoản còn hoạt động, nên token dựng tay phải có
        /// phiên và tài khoản trong store. Tài khoản đã có (tạo qua API hoặc dữ liệu mẫu) thì giữ nguyên.
        /// </summary>
        public static Guid SeedSession(WebApplication app, Guid userId, string role)
        {
            var users = app.Services.GetRequiredService<InMemoryStore<tai_khoan>>();
            if (users.Find(userId) is null)
            {
                users.TryAdd(new tai_khoan { Id = userId, ten_dang_nhap = "test.user", ho_ten = "Test", vai_tro = role });
            }

            var session = new phien_dang_nhap
            {
                tai_khoan_id = userId,
                ngay_het_han = DateTime.UtcNow.AddDays(1),
                ngay_het_han_toi_da = DateTime.UtcNow.AddDays(1),
            };
            app.Services.GetRequiredService<InMemoryStore<phien_dang_nhap>>().TryAdd(session);
            return session.Id;
        }

        /// <summary>Client mang access token thật của đúng tài khoản <paramref name="userId"/>, để thử quyền theo dữ liệu trong DB.</summary>
        public static HttpClient ClientFor(WebApplication app, Guid userId, string role)
        {
            var token = app.Services.GetRequiredService<ITokenIssuer>()
                .CreateAccessToken(new AccessTokenRequest(userId, "test.user", role, SeedSession(app, userId, role)));
            var client = app.GetTestClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            return client;
        }

        /// <summary>Client mang access token thật do app cấp, với vai trò cho trước.</summary>
        public static HttpClient ClientWithRole(WebApplication app, string role)
        {
            var userId = Guid.CreateVersion7();
            var token = app.Services.GetRequiredService<ITokenIssuer>()
                .CreateAccessToken(new AccessTokenRequest(userId, "test.user", role, SeedSession(app, userId, role)));
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

    /// <summary>Mỗi endpoint dùng một policy phân quyền, và một endpoint kiểm tra quyền trên dữ liệu học viên: /api/v1/web/phan-quyen/...</summary>
    [Route("phan-quyen")]
    public sealed class PhanQuyenTestController : BaseApiController
    {
        [HttpGet("admin")]
        [Authorize(Policy = AppPolicies.RequireAdmin)]
        public IActionResult Admin() => Ok();

        [HttpGet("ho-so")]
        [Authorize(Policy = AppPolicies.RequireProfile)]
        public IActionResult HoSo() => Ok();

        [HttpGet("pt")]
        [Authorize(Policy = AppPolicies.RequirePtProfile)]
        public IActionResult Pt() => Ok();

        [HttpGet("pt-xac-minh")]
        [Authorize(Policy = AppPolicies.RequireVerifiedPt)]
        public IActionResult PtXacMinh() => Ok();

        /// <summary>Policy cho biết "là PT đã xác minh"; hàm BL mới biết "đúng học viên của PT này".</summary>
        [HttpGet("hoc-vien/{hocVienId:guid}/dinh-duong")]
        [Authorize(Policy = AppPolicies.RequireVerifiedPt)]
        public async Task<IActionResult> DinhDuong(
            Guid hocVienId, [FromServices] TruyCapService truyCap, [FromServices] ICurrentUser currentUser, CancellationToken cancellationToken)
        {
            await truyCap.BLKiemTraQuanLyHocVienAsync(currentUser.UserId!.Value, hocVienId, PhamViPhuTrach.dinh_duong, cancellationToken);
            return Ok();
        }
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
