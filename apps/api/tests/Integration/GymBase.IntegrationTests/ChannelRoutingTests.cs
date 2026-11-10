using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace GymBase.IntegrationTests;

/// <summary>
/// Một host phục vụ hai kênh: controller web (Controllers/) ở /api/v1/web, controller mobile
/// (Mob/Controllers/) ở /api/v1/mob, kể cả khi hai controller trùng tên.
/// </summary>
public sealed class ChannelRoutingTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Theory]
    [InlineData("/api/v1/web/testusers")]
    [InlineData("/api/v1/mob/testusers")]
    public async Task Both_channels_serve_same_data(string path)
    {
        var response = await _client.GetAsync($"{path}?page_size=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, page.GetProperty("meta").GetProperty("total").GetInt32());
    }

    [Theory]
    [InlineData("/api/v1/web/testusers", "hv.web")]
    [InlineData("/api/v1/mob/testusers", "hv.mob")]
    public async Task Created_location_points_to_same_channel(string path, string taiKhoan)
    {
        var response = await _client.PostAsJsonAsync(path, new { tai_khoan = taiKhoan, mat_khau = "Mk-thu-nghiem-1", ho_ten = "Kênh", vai_tro = "Trainee" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.StartsWith(path, response.Headers.Location!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/web/v1/testusers")]
    [InlineData("/api/mobile/v1/testusers")]
    public async Task Old_routes_are_gone(string path)
    {
        using var client = TestApp.ClientWithRole(_app, "Trainee");

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
