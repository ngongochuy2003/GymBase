using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Base.Auth;
using GymBase.Entities.DI;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>
/// API danh mục cho app mobile: <c>GET /api/v1/mob/danh-muc</c>. Dữ liệu lấy từ <see cref="InMemoryDanhMucRepository"/>,
/// nạp sẵn đúng dữ liệu tham chiếu của migration <c>ThemBangDanhMuc</c>.
/// </summary>
public sealed class DanhMucTests : IAsyncLifetime
{
    private const string DanhMuc = "/api/v1/mob/danh-muc";

    private WebApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApp.StartAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Tra_ba_loai_dang_dung_nhom_theo_ma_dinh_danh()
    {
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var response = await client.GetAsync($"{DanhMuc}?ma_dinh_danh=muc_tieu_tap_luyen,khu_vuc,chuyen_mon_pt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var nhom = body.EnumerateArray().ToList();
        Assert.Equal(["chuyen_mon_pt", "khu_vuc", "muc_tieu_tap_luyen"], nhom.Select(n => n.GetProperty("ma_dinh_danh").GetString()));
        Assert.All(nhom, n => Assert.Equal(5, n.GetProperty("items").GetArrayLength()));

        // App hiển thị ten nhưng gửi lại ma khi lưu hồ sơ, nên cả hai phải có trong response.
        var tangCo = nhom.Single(n => n.GetProperty("ma_dinh_danh").GetString() == "muc_tieu_tap_luyen")
            .GetProperty("items")
            .EnumerateArray()
            .Single(i => i.GetProperty("ma").GetString() == "tang_co");
        Assert.Equal("Tăng cơ", tangCo.GetProperty("ten").GetString());
        Assert.Equal(1, tangCo.GetProperty("cap").GetInt32());
        Assert.Equal(JsonValueKind.Null, tangCo.GetProperty("danh_muc_cap_tren_id").ValueKind);
    }

    [Fact]
    public async Task Loc_theo_ma_dinh_danh()
    {
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var body = await client.GetFromJsonAsync<JsonElement>($"{DanhMuc}?ma_dinh_danh=khu_vuc");

        var nhom = body.EnumerateArray().Single();
        Assert.Equal("khu_vuc", nhom.GetProperty("ma_dinh_danh").GetString());
        Assert.Equal(5, nhom.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Bo_qua_muc_co_tinh_trang_bang_false()
    {
        var store = _app.Services.GetRequiredService<InMemoryStore<danh_muc>>();
        store.TryAdd(new danh_muc
        {
            ma_dinh_danh = "muc_tieu_tap_luyen",
            ma = "yoga",
            ten = "Yoga",
            cap = 1,
            tinh_trang = false,
        });
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var body = await client.GetFromJsonAsync<JsonElement>($"{DanhMuc}?ma_dinh_danh=muc_tieu_tap_luyen");

        var items = body.EnumerateArray().Single().GetProperty("items").EnumerateArray().ToList();
        Assert.DoesNotContain(items, i => i.GetProperty("ma").GetString() == "yoga");
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task Tra_401_khi_chua_dang_nhap()
    {
        var client = _app.GetTestClient();

        var response = await client.GetAsync(DanhMuc);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
