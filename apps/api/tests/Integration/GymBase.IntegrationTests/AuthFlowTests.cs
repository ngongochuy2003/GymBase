using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GymBase.Base.Auth;
using GymBase.Base.Hosting.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace GymBase.IntegrationTests;

/// <summary>Xác thực và phân quyền đi qua controller thật và JWT thật.</summary>
public sealed class AuthFlowTests : IAsyncLifetime
{
    private const string Auth = "/api/v1/web/auth";
    private const string Users = "/api/v1/web/users";
    private const string Password = "Mk-thu-nghiem-1";

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private async Task CreateUser(string userName)
    {
        var body = new { ten_dang_nhap = userName, mat_khau = Password, ho_ten = "Thử", vai_tro = AppRoles.User };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/web/testusers", body)).StatusCode);
    }

    private async Task<JsonElement> LoginAs(string userName)
    {
        await CreateUser(userName);
        var response = await _client.PostAsJsonAsync($"{Auth}/login", new { ten_dang_nhap = userName, mat_khau = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<HttpResponseMessage> GetWithToken(string path, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(code, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Anonymous_request_gets_401_problem_details()
    {
        await AssertProblem(await _client.GetAsync(Users), HttpStatusCode.Unauthorized, "UNAUTHORIZED");
    }

    [Fact]
    public async Task Health_is_public()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Non_admin_gets_403_on_user_management()
    {
        using var client = TestApp.ClientWithRole(_app, AppRoles.User);

        await AssertProblem(await client.GetAsync(Users), HttpStatusCode.Forbidden, "FORBIDDEN");
        await AssertProblem(await client.GetAsync("/api/v1/mob/users"), HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task Admin_can_manage_users()
    {
        using var client = TestApp.ClientWithRole(_app, AppRoles.Admin);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Users)).StatusCode);
    }

    [Fact]
    public async Task Login_token_works_on_me_and_contains_no_personal_data()
    {
        var login = await LoginAs("hv.token");
        var token = login.GetProperty("access_token").GetString()!;

        var me = await (await GetWithToken($"{Auth}/me", token)).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("hv.token", me.GetProperty("ten_dang_nhap").GetString());
        Assert.Equal(900, login.GetProperty("expires_in").GetInt32());

        var parts = token.Split('.');
        var header = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[0])));
        var payload = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
        Assert.Contains("at+jwt", header);
        Assert.Contains("HS256", header);
        Assert.Contains("\"preferred_username\":\"hv.token\"", payload);
        Assert.DoesNotContain("email", payload);
        Assert.DoesNotContain("ho_ten", payload);
    }

    private static string Pad(string base64Url)
    {
        var value = base64Url.Replace('-', '+').Replace('_', '/');
        return value.PadRight(value.Length + ((4 - (value.Length % 4)) % 4), '=');
    }

    [Fact]
    public async Task Wrong_login_gives_the_same_response_for_wrong_password_and_unknown_user()
    {
        await CreateUser("hv.saimk");

        var wrong = await _client.PostAsJsonAsync($"{Auth}/login", new { ten_dang_nhap = "hv.saimk", mat_khau = "sai-mat-khau" });
        var unknown = await _client.PostAsJsonAsync($"{Auth}/login", new { ten_dang_nhap = "khong.co", mat_khau = "sai-mat-khau" });

        await AssertProblem(wrong, HttpStatusCode.Unauthorized, "AUTH_INVALID_CREDENTIALS");
        await AssertProblem(unknown, HttpStatusCode.Unauthorized, "AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Refresh_returns_new_tokens()
    {
        var login = await LoginAs("hv.refresh");
        var refreshToken = login.GetProperty("refresh_token").GetString();

        var response = await _client.PostAsJsonAsync($"{Auth}/refresh", new { refresh_token = refreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual(refreshToken, body.GetProperty("refresh_token").GetString());
    }

    [Fact]
    public async Task Invalid_refresh_token_gets_401()
    {
        var response = await _client.PostAsJsonAsync($"{Auth}/refresh", new { refresh_token = "khong-hop-le" });

        await AssertProblem(response, HttpStatusCode.Unauthorized, "UNAUTHORIZED");
    }

    [Fact]
    public async Task Logout_stops_the_refresh_token_working()
    {
        var login = await LoginAs("hv.logout");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Auth}/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.GetProperty("access_token").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(request)).StatusCode);

        var refresh = await _client.PostAsJsonAsync($"{Auth}/refresh", new { refresh_token = login.GetProperty("refresh_token").GetString() });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = "b3RoZXItc2lnbmluZy1rZXktb2YtdGhpcnR5LXR3by1ieXRlcw==" })
            .Build();
        var forged = new JwtTokenIssuer(JwtSettings.From(configuration), TimeProvider.System)
            .CreateAccessToken(new AccessTokenRequest(Guid.CreateVersion7(), "gia.mao", AppRoles.Admin, Guid.CreateVersion7()));

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken(Users, forged.Token)).StatusCode);
    }

    [Fact]
    public async Task Token_with_edited_role_is_rejected()
    {
        var token = (await LoginAs("hv.suarole")).GetProperty("access_token").GetString()!;
        var parts = token.Split('.');
        var payload = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1]))).Replace(AppRoles.User, AppRoles.Admin);
        var edited = $"{parts[0]}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=').Replace('+', '-').Replace('/', '_')}.{parts[2]}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken(Users, edited)).StatusCode);
    }

    [Fact]
    public async Task Unsigned_token_is_rejected()
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"none","typ":"at+jwt"}""")).TrimEnd('=');
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            """{"iss":"gymbase-api","aud":"gymbase-api","sub":"x","role":"Admin","exp":4102444800}""")).TrimEnd('=');

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithToken(Users, $"{header}.{payload}.")).StatusCode);
    }

    [Fact]
    public async Task Login_is_rate_limited()
    {
        HttpResponseMessage last = null!;
        for (var i = 0; i < 21; i++)
        {
            last = await _client.PostAsJsonAsync($"{Auth}/login", new { ten_dang_nhap = $"khong.co.{i}", mat_khau = "sai-mat-khau" });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
    }

    [Fact]
    public void Short_signing_key_is_rejected_at_startup()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = "c2hvcnQ=" })
            .Build();

        Assert.Throws<InvalidOperationException>(() => JwtSettings.From(configuration));
    }
}
