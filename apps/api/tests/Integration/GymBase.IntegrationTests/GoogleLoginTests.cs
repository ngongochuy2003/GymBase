using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Base.Auth;
using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>Đăng nhập và đăng ký bằng Google qua controller thật; bộ xác minh ID token là bản giả ở ranh giới với Google.</summary>
public sealed class GoogleLoginTests : IAsyncLifetime
{
    private const string Web = "/api/v1/web/auth";
    private const string Mob = "/api/v1/mob/auth";
    private const string Password = "Mk-thu-nghiem-1";

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private FakeGoogleIdTokenVerifier _google = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
        _google = _app.Services.GetRequiredService<FakeGoogleIdTokenVerifier>();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private string Accept(
        string subject = "google-sub-1",
        string? email = "Nguoi.Moi@Gmail.Test",
        bool emailVerified = true,
        string? name = "Người Mới")
    {
        var token = $"id-token-{Guid.NewGuid()}";
        _google.Accept(token, new GoogleIdentity(subject, email, emailVerified, name));
        return token;
    }

    private Task<HttpResponseMessage> Google(string idToken, string path = Web) =>
        _client.PostAsJsonAsync($"{path}/google", new { id_token = idToken });

    private static async Task<JsonElement> Json(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(code, (await Json(response)).GetProperty("code").GetString());
    }

    private async Task<T> WithScope<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _app.Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    private Task<tai_khoan?> StoredUser(string email) =>
        WithScope(services => services.GetRequiredService<ITaiKhoanRepository>().DLFindByEmailAsync(email, CancellationToken.None));

    private Task<dang_nhap_ngoai?> StoredLink(string subject) =>
        WithScope(services => services.GetRequiredService<IDangNhapNgoaiRepository>()
            .DLFindByMaNgoaiAsync(NhaCungCap.google, subject, CancellationToken.None));

    private async Task RegisterWithPassword(string email) =>
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync($"{Web}/register", new
        {
            ho_ten = "Có mật khẩu",
            email,
            so_dien_thoai = "0912345678",
            mat_khau = Password,
            dong_y_dieu_khoan = true,
        })).StatusCode);

    [Theory]
    [InlineData(Web)]
    [InlineData(Mob)]
    public async Task First_sign_in_creates_the_account_and_marks_it_as_new(string path)
    {
        var response = await Google(Accept(), path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await Json(response);
        Assert.True(body.GetProperty("tai_khoan_moi").GetBoolean());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrEmpty(body.GetProperty("refresh_token").GetString()));
        var user = body.GetProperty("user");
        Assert.Equal("nguoi.moi@gmail.test", user.GetProperty("email").GetString());
        Assert.Equal("Người Mới", user.GetProperty("ho_ten").GetString());
        Assert.Equal("User", user.GetProperty("vai_tro").GetString());
        Assert.DoesNotContain("mat_khau", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task First_sign_in_stores_a_passwordless_account_linked_by_google_sub_with_the_consent_time()
    {
        await Google(Accept(subject: "sub-luu"));

        var user = (await StoredUser("nguoi.moi@gmail.test"))!;
        Assert.Null(user.mat_khau_hash);
        Assert.Null(user.ten_dang_nhap);
        Assert.NotNull(user.ngay_dong_y_dieu_khoan);
        var link = (await StoredLink("sub-luu"))!;
        Assert.Equal(user.Id, link.tai_khoan_id);
        Assert.Equal(DateTimeKind.Utc, link.ngay_tao.Kind);
    }

    [Fact]
    public async Task Token_from_google_sign_in_works_on_me()
    {
        var token = (await Json(await Google(Accept()))).GetProperty("access_token").GetString();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Web}/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal("nguoi.moi@gmail.test", (await Json(me)).GetProperty("email").GetString());
    }

    [Fact]
    public async Task Second_sign_in_reuses_the_account_and_is_not_marked_new()
    {
        var first = await Json(await Google(Accept()));

        var second = await Json(await Google(Accept()));

        Assert.False(second.GetProperty("tai_khoan_moi").GetBoolean());
        Assert.Equal(first.GetProperty("user").GetProperty("id").GetString(), second.GetProperty("user").GetProperty("id").GetString());
    }

    [Fact]
    public async Task The_account_is_found_by_google_sub_even_when_the_google_email_changes()
    {
        var first = await Json(await Google(Accept(subject: "sub-doi-email", email: "cu@gmail.test")));

        var second = await Json(await Google(Accept(subject: "sub-doi-email", email: "moi@gmail.test")));

        Assert.False(second.GetProperty("tai_khoan_moi").GetBoolean());
        Assert.Equal(first.GetProperty("user").GetProperty("id").GetString(), second.GetProperty("user").GetProperty("id").GetString());
        Assert.Equal("cu@gmail.test", second.GetProperty("user").GetProperty("email").GetString());
        Assert.Null(await StoredUser("moi@gmail.test"));
    }

    [Fact]
    public async Task Missing_google_name_falls_back_to_the_part_of_the_email_before_the_at_sign()
    {
        var response = await Google(Accept(name: null));

        Assert.Equal("nguoi.moi", (await Json(response)).GetProperty("user").GetProperty("ho_ten").GetString());
    }

    [Fact]
    public async Task A_google_name_longer_than_the_column_is_cut_instead_of_failing()
    {
        var response = await Google(Accept(name: new string('A', 150)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(100, (await Json(response)).GetProperty("user").GetProperty("ho_ten").GetString()!.Length);
    }

    [Fact]
    public async Task Email_of_a_password_account_is_refused_and_nothing_is_created()
    {
        await RegisterWithPassword("co.mat.khau@gmail.test");
        var token = Accept(subject: "sub-trung-mk", email: "Co.Mat.Khau@Gmail.Test");

        await AssertProblem(await Google(token), HttpStatusCode.Conflict, "AUTH_EMAIL_USED_PASSWORD");
        await AssertProblem(await Google(token), HttpStatusCode.Conflict, "AUTH_EMAIL_USED_PASSWORD");

        Assert.Null(await StoredLink("sub-trung-mk"));
    }

    [Fact]
    public async Task Email_of_another_google_account_is_refused()
    {
        await Google(Accept(subject: "sub-a", email: "chung@gmail.test"));

        await AssertProblem(await Google(Accept(subject: "sub-b", email: "chung@gmail.test")), HttpStatusCode.Conflict, "USER_EMAIL_TAKEN");
    }

    [Fact]
    public async Task Password_registration_with_a_google_accounts_email_is_refused()
    {
        await Google(Accept(email: "da.dung.google@gmail.test"));

        var response = await _client.PostAsJsonAsync($"{Web}/register", new
        {
            ho_ten = "Trùng email",
            email = "da.dung.google@gmail.test",
            so_dien_thoai = "0912345678",
            mat_khau = Password,
            dong_y_dieu_khoan = true,
        });

        await AssertProblem(response, HttpStatusCode.Conflict, "USER_EMAIL_TAKEN");
    }

    [Fact]
    public async Task A_google_only_account_cannot_sign_in_with_a_password()
    {
        await Google(Accept(email: "chi.google@gmail.test"));

        var login = await _client.PostAsJsonAsync($"{Web}/login", new { email = "chi.google@gmail.test", mat_khau = Password });

        await AssertProblem(login, HttpStatusCode.Unauthorized, "AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Refresh_token_from_google_sign_in_works()
    {
        var refreshToken = (await Json(await Google(Accept()))).GetProperty("refresh_token").GetString();

        var refreshed = await _client.PostAsJsonAsync($"{Web}/refresh", new { refresh_token = refreshToken });

        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
    }

    [Fact]
    public async Task An_id_token_google_does_not_vouch_for_is_rejected()
    {
        await AssertProblem(await Google("token-khong-hop-le"), HttpStatusCode.Unauthorized, "AUTH_GOOGLE_TOKEN_INVALID");
    }

    [Fact]
    public async Task An_empty_id_token_is_a_validation_error()
    {
        await AssertProblem(await Google(""), HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("chua.xac.minh@gmail.test", false)]
    public async Task A_new_account_needs_a_verified_google_email(string? email, bool emailVerified)
    {
        var response = await Google(Accept(subject: "sub-khong-email", email: email, emailVerified: emailVerified));

        await AssertProblem(response, HttpStatusCode.Unauthorized, "AUTH_GOOGLE_EMAIL_UNVERIFIED");
        Assert.Null(await StoredLink("sub-khong-email"));
    }

    [Fact]
    public async Task A_disabled_account_cannot_sign_in_with_google()
    {
        var created = await Json(await Google(Accept(subject: "sub-khoa")));
        var userId = Guid.Parse(created.GetProperty("user").GetProperty("id").GetString()!);
        await WithScope(async services =>
        {
            var users = services.GetRequiredService<ITaiKhoanRepository>();
            var user = (await users.DLGetByIdAsync(userId, CancellationToken.None))!;
            user.dang_hoat_dong = false;
            await users.DLUpdateAsync(user, CancellationToken.None);
            return 0;
        });

        var again = await Google(Accept(subject: "sub-khoa"));

        Assert.Equal(HttpStatusCode.Unauthorized, again.StatusCode);
    }

    [Fact]
    public async Task Google_sign_in_is_rate_limited()
    {
        HttpResponseMessage last = null!;
        for (var i = 0; i < 21; i++)
        {
            last = await Google($"token-sai-{i}");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
    }
}
