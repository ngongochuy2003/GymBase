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
    public async Task Created_location_points_to_same_channel(string path, string tenDangNhap)
    {
        var response = await _client.PostAsJsonAsync(path, new { ten_dang_nhap = tenDangNhap, mat_khau = "Mk-thu-nghiem-1", ho_ten = "Kênh", vai_tro = "User" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.StartsWith(path, response.Headers.Location!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Controller đặt <c>[Route("buoi-tap")]</c> chỉ phục vụ URL tường minh có gạch nối; route lấy theo tên controller
    /// (<c>/testbuoitap</c>) của lớp gốc không còn. Hướng dẫn: docs/api/huong-dan-them-api-nghiep-vu.md.
    /// </summary>
    [Fact]
    public async Task Explicit_route_replaces_route_from_controller_name()
    {
        var id = Guid.CreateVersion7();

        using var client = TestApp.ClientWithRole(_app, "User");

        var explicitRoute = await client.PostAsync($"/api/v1/web/buoi-tap/{id}/xac-nhan", null);
        var controllerNameRoute = await client.PostAsync($"/api/v1/web/testbuoitap/{id}/xac-nhan", null);

        Assert.Equal(HttpStatusCode.OK, explicitRoute.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, controllerNameRoute.StatusCode);
    }

    [Theory]
    [InlineData("/api/web/v1/testusers")]
    [InlineData("/api/mobile/v1/testusers")]
    public async Task Old_routes_are_gone(string path)
    {
        using var client = TestApp.ClientWithRole(_app, "User");

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
