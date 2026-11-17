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
        string? phone = "0912345678",
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
    public async Task Register_creates_the_account_without_a_session(string path)
    {
        var response = await Register(Body(), path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("access_token", text);
        Assert.DoesNotContain("refresh_token", text);
        Assert.DoesNotContain("mat_khau", text);
        var body = await Json(response);
        Assert.Equal("moi@gymbase.test", body.GetProperty("email").GetString());
        Assert.Equal("User", body.GetProperty("vai_tro").GetString());
        Assert.True(body.GetProperty("is_dang_nhap_lan_dau").GetBoolean());
    }

    [Fact]
    public async Task Registered_account_can_log_in_with_its_email_and_still_has_the_first_login_flag()
    {
        await Register(Body(email: "Dang.Nhap@GymBase.Test"));

        var login = await _client.PostAsJsonAsync($"{Web}/login", new { email = "dang.nhap@gymbase.test", mat_khau = Password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.True((await Json(login)).GetProperty("user").GetProperty("is_dang_nhap_lan_dau").GetBoolean());
    }

    [Theory]
    [InlineData(Web)]
    [InlineData(Mob)]
    public async Task Completing_the_first_login_clears_the_flag_for_later_logins(string path)
    {
        await Register(Body(), path);
        var login = await Json(await _client.PostAsJsonAsync($"{path}/login", new { email = "moi@gymbase.test", mat_khau = Password }));
        var token = login.GetProperty("access_token").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{path}/hoan-tat-dang-nhap-lan-dau");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var done = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, done.StatusCode);
        Assert.False((await Json(done)).GetProperty("is_dang_nhap_lan_dau").GetBoolean());
        var again = await Json(await _client.PostAsJsonAsync($"{path}/login", new { email = "moi@gymbase.test", mat_khau = Password }));
        Assert.False(again.GetProperty("user").GetProperty("is_dang_nhap_lan_dau").GetBoolean());
    }

    [Fact]
    public async Task Completing_the_first_login_requires_a_session()
    {
        var response = await _client.PostAsync($"{Web}/hoan-tat-dang-nhap-lan-dau", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_stores_the_hash_and_normalised_email_and_phone()
    {
        await Register(Body(email: "  Luu.Tru@GymBase.Test ", phone: "+84 912 345 678"));

        var user = await StoredUser("luu.tru@gymbase.test");
        Assert.Equal("0912345678", user.so_dien_thoai);
        Assert.NotNull(user.mat_khau_hash);
        Assert.NotEqual(Password, user.mat_khau_hash);
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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Phone_is_optional_and_stored_as_null(string? phone)
    {
        var response = await Register(Body(email: "khong.co.so@gymbase.test", phone: phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null((await StoredUser("khong.co.so@gymbase.test")).so_dien_thoai);
    }

    [Fact]
    public async Task Omitted_phone_field_is_accepted()
    {
        var body = new
        {
            ho_ten = "Không có số",
            email = "thieu.so@gymbase.test",
            mat_khau = Password,
            dong_y_dieu_khoan = true,
        };

        Assert.Equal(HttpStatusCode.OK, (await Register(body)).StatusCode);
        Assert.Null((await StoredUser("thieu.so@gymbase.test")).so_dien_thoai);
    }

    [Theory]
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

    [Fact]
    public async Task Password_shorter_than_eight_characters_is_rejected()
    {
        var response = await Register(Body(password: "ngan"));

        await AssertProblem(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED", "mat_khau");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("matkhau123")]
    [InlineData("Mk-phuong.thao-1")]
    public async Task Only_length_matters_so_common_or_email_containing_passwords_are_accepted(string password)
    {
        var response = await Register(Body(email: "phuong.thao@gymbase.test", password: password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
