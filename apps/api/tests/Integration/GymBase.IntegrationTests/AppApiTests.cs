using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>Cấu hình dùng chung trong AddAppApi / UseAppApi: CORS, URL chữ thường, người dùng hiện tại.</summary>
public sealed class AppApiTests : IAsyncLifetime
{
    private const string Users = "/api/v1/web/testusers";

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private async Task<HttpResponseMessage> GetFrom(string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Users);
        request.Headers.Add("Origin", origin);
        return await _client.SendAsync(request);
    }

    [Fact]
    public async Task Cors_allows_configured_origin()
    {
        var response = await GetFrom(TestApp.AllowedOrigin);

        Assert.Equal([TestApp.AllowedOrigin], response.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_does_not_allow_other_origin()
    {
        var response = await GetFrom("https://evil.example");

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_answers_preflight_for_configured_origin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, Users);
        request.Headers.Add("Origin", TestApp.AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "DELETE");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains("DELETE", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
    }

    [Theory]
    [InlineData("*")]
    [InlineData(" * ")]
    public void Wildcard_origin_is_rejected_at_startup(string origin)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"{AppApiExtensions.CorsOriginsSection}:0"] = origin })
            .Build();

        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddAppApi(configuration));
    }

    [Fact]
    public async Task Location_header_is_lowercase()
    {
        var body = new { ten_dang_nhap = "hv.lowercase", email = "hv.lowercase@gymbase.test", mat_khau = "Mk-thu-nghiem-1", ho_ten = "Thử", vai_tro = "User" };

        var response = await _client.PostAsJsonAsync(Users, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var path = response.Headers.Location!.AbsolutePath;
        Assert.Equal(path.ToLowerInvariant(), path);
    }

    private static HttpCurrentUser CurrentUserWith(ClaimsIdentity identity) =>
        new(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } });

    [Fact]
    public void Current_user_reads_claims_of_authenticated_request()
    {
        var id = Guid.CreateVersion7();

        var user = CurrentUserWith(new ClaimsIdentity([new Claim("sub", id.ToString()), new Claim("preferred_username", "nv.thu")], "Test"));

        Assert.Equal(id, user.UserId);
        Assert.Equal("nv.thu", user.UserName);
    }

    [Fact]
    public void Current_user_ignores_claims_of_unauthenticated_identity()
    {
        var user = CurrentUserWith(new ClaimsIdentity([new Claim("preferred_username", "gia.mao")]));

        Assert.Null(user.UserName);
        Assert.Null(user.UserId);
    }

    [Fact]
    public void Current_user_is_empty_outside_a_request()
    {
        var user = new HttpCurrentUser(new HttpContextAccessor());

        Assert.Null(user.UserName);
    }
}
