using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>Người mới tự đăng ký bằng mật khẩu qua controller thật, trên cả kênh web và mobile.</summary>
public sealed class RegisterTests : IAsyncLifetime
{
    private const string Web = "/api/v1/web/auth";
    private const string Mob = "/api/v1/mob/auth";
    private const string Password = "Mk-thu-nghiem-1";

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private static object Body(
        string email = "moi@gymbase.test",
        string phone = "0912345678",
        string password = Password,
        bool agreed = true,
        string name = "Người mới") => new
        {
            ho_ten = name,
            email,
            so_dien_thoai = phone,
            mat_khau = password,
            dong_y_dieu_khoan = agreed,
        };

    private Task<HttpResponseMessage> Register(object body, string path = Web) =>
        _client.PostAsJsonAsync($"{path}/register", body);

    private static async Task<JsonElement> Json(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string code, params string[] errorFields)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await Json(response);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        if (errorFields.Length > 0)
        {
            var fields = problem.GetProperty("errors").EnumerateObject().Select(p => p.Name).Order().ToArray();
            Assert.Equal(errorFields.Order().ToArray(), fields);
        }
    }

    private async Task<GymBase.Entities.Auth.tai_khoan> StoredUser(string email)
    {
        using var scope = _app.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<ITaiKhoanRepository>().DLFindByEmailAsync(email, CancellationToken.None))!;
    }

    [Theory]
    [InlineData(Web)]
    [InlineData(Mob)]
    public async Task Register_returns_tokens_and_marks_the_account_as_new(string path)
    {
        var response = await Register(Body(), path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await Json(response);
        Assert.False(string.IsNullOrEmpty(body.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrEmpty(body.GetProperty("refresh_token").GetString()));
        Assert.True(body.GetProperty("tai_khoan_moi").GetBoolean());
        var user = body.GetProperty("user");
        Assert.Equal("moi@gymbase.test", user.GetProperty("email").GetString());
        Assert.Equal("User", user.GetProperty("vai_tro").GetString());
        Assert.DoesNotContain("mat_khau", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Token_from_register_works_without_logging_in_again()
    {
        var token = (await Json(await Register(Body()))).GetProperty("access_token").GetString();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Web}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal("moi@gymbase.test", (await Json(me)).GetProperty("email").GetString());
    }

    [Fact]
    public async Task Registered_account_can_log_in_with_its_email_and_login_is_not_marked_new()
    {
        await Register(Body(email: "Dang.Nhap@GymBase.Test"));

        var login = await _client.PostAsJsonAsync($"{Web}/login", new { email = "dang.nhap@gymbase.test", mat_khau = Password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False((await Json(login)).GetProperty("tai_khoan_moi").GetBoolean());
    }

    [Fact]
    public async Task Register_stores_the_hash_normalised_email_phone_and_the_consent_time()
    {
        var before = DateTime.UtcNow;

        await Register(Body(email: "  Luu.Tru@GymBase.Test ", phone: "+84 912 345 678"));

        var user = await StoredUser("luu.tru@gymbase.test");
        Assert.Equal("0912345678", user.so_dien_thoai);
        Assert.NotNull(user.mat_khau_hash);
        Assert.NotEqual(Password, user.mat_khau_hash);
        Assert.NotNull(user.ngay_dong_y_dieu_khoan);
        Assert.Equal(DateTimeKind.Utc, user.ngay_dong_y_dieu_khoan!.Value.Kind);
        Assert.InRange(user.ngay_dong_y_dieu_khoan.Value, before.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task Taken_email_is_rejected_whatever_the_case()
    {
        await Register(Body(email: "trung@gymbase.test"));

        var again = await Register(Body(email: " TRUNG@gymbase.test "));

        await AssertProblem(again, HttpStatusCode.Conflict, "USER_EMAIL_TAKEN", "email");
    }

    [Fact]
    public async Task Email_of_an_account_created_by_admin_is_taken_too()
    {
        // Dữ liệu mẫu có sẵn admin@gymbase.local.
        await AssertProblem(await Register(Body(email: "admin@gymbase.local")), HttpStatusCode.Conflict, "USER_EMAIL_TAKEN", "email");
    }

    [Fact]
    public async Task Not_agreeing_to_the_terms_is_rejected_and_creates_nothing()
    {
        await AssertProblem(await Register(Body(agreed: false)), HttpStatusCode.BadRequest, "VALIDATION_FAILED", "dong_y_dieu_khoan");

        Assert.Null(await StoredUser("moi@gymbase.test"));
    }

    [Fact]
    public async Task Each_invalid_field_gets_its_own_error()
    {
        var response = await Register(Body(email: "khong-phai-email", phone: "12345", password: "ngan", name: ""));

        await AssertProblem(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED", "ho_ten", "email", "so_dien_thoai", "mat_khau");
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0912 345 678")]
    [InlineData("091.234.5678")]
    [InlineData("+84912345678")]
    [InlineData("84912345678")]
    [InlineData("0356789012")]
    public async Task Vietnamese_mobile_numbers_are_accepted(string phone)
    {
        Assert.Equal(HttpStatusCode.OK, (await Register(Body(phone: phone))).StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0123456789")]
    [InlineData("091234567")]
    [InlineData("09123456789")]
    [InlineData("+15551234567")]
    [InlineData("abcdefghij")]
    public async Task Other_phone_numbers_are_rejected(string phone)
    {
        await AssertProblem(await Register(Body(phone: phone)), HttpStatusCode.BadRequest, "VALIDATION_FAILED", "so_dien_thoai");
    }

    [Fact]
    public async Task Same_phone_number_can_register_twice()
    {
        await Register(Body(email: "mot@gymbase.test"));

        Assert.Equal(HttpStatusCode.OK, (await Register(Body(email: "hai@gymbase.test"))).StatusCode);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("matkhau123")]
    [InlineData("Mk-phuong.thao-1")]
    public async Task Weak_or_email_containing_passwords_are_rejected(string password)
    {
        var response = await Register(Body(email: "phuong.thao@gymbase.test", password: password));

        await AssertProblem(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED", "mat_khau");
    }

    [Fact]
    public async Task Null_fields_are_validation_errors_not_server_errors()
    {
        var body = new { ho_ten = (string?)null, email = (string?)null, so_dien_thoai = (string?)null, mat_khau = (string?)null, dong_y_dieu_khoan = true };

        var response = await Register(body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Referral_code_is_accepted_and_ignored()
    {
        var body = new
        {
            ho_ten = "Có mã",
            email = "co.ma@gymbase.test",
            so_dien_thoai = "0912345678",
            mat_khau = Password,
            dong_y_dieu_khoan = true,
            ma_gioi_thieu = "KHOI123",
        };

        Assert.Equal(HttpStatusCode.OK, (await Register(body)).StatusCode);
    }

    [Fact]
    public async Task Register_is_rate_limited()
    {
        HttpResponseMessage last = null!;
        for (var i = 0; i < 21; i++)
        {
            last = await Register(Body(email: $"nhanh.{i}@gymbase.test"));
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
    }
}
