using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace GymBase.IntegrationTests;

/// <summary>Tham số sort, filter đi từ query string qua controller, dùng danh sách cột thật của người dùng.</summary>
public sealed class SortFilterTests : IAsyncLifetime
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

    private async Task<string[]> UserNames(string query)
    {
        var response = await _client.GetAsync($"{Users}?{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").EnumerateArray()
            .Select(u => u.GetProperty("tai_khoan").GetString()!)
            .ToArray();
    }

    [Fact]
    public async Task Sort_changes_order()
    {
        Assert.Equal(["admin", "hv.huy", "hv.phuong", "pt.lan", "pt.minh"], await UserNames("sort=tai_khoan"));
    }

    [Fact]
    public async Task Filter_combines_with_sort()
    {
        Assert.Equal(["pt.minh", "pt.lan"], await UserNames("filter=vai_tro:eq:Trainer&sort=-tai_khoan"));
    }

    [Fact]
    public async Task Soft_deleted_user_stays_hidden_when_filtering()
    {
        Assert.Equal(["hv.huy"], await UserNames("filter=vai_tro:eq:Trainee,dang_hoat_dong:eq:true"));
    }

    [Theory]
    [InlineData("sort=mat_khau_hash", "sort")]
    [InlineData("filter=is_deleted:eq:true", "filter")]
    [InlineData("filter=vai_tro", "filter")]
    public async Task Invalid_sort_or_filter_returns_400_with_field_error(string query, string field)
    {
        var response = await _client.GetAsync($"{Users}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION_FAILED", json.RootElement.GetProperty("code").GetString());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty(field, out _));
    }
}
