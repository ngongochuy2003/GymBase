using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Base.Auth;
using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.Entities.DI;
using GymBase.ModuleContracts.DanhMuc;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>
/// API danh mục cho app mobile: <c>GET /api/v1/mob/danh-muc</c>. Dữ liệu lấy từ <see cref="InMemoryDanhMucRepository"/>,
/// nạp sẵn dữ liệu tham chiếu như migration, riêng khu vực chỉ vài tỉnh/thành.
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
    public async Task Khong_co_ma_cap_tren_thi_chi_tra_cap_1()
    {
        ThemPhuongCuaHaNoi();
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var body = await client.GetFromJsonAsync<JsonElement>($"{DanhMuc}?ma_dinh_danh=khu_vuc");

        var items = body.EnumerateArray().Single().GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(5, items.Count);
        Assert.DoesNotContain(items, i => i.GetProperty("ma").GetString() == "px_10101003");
    }

    [Fact]
    public async Task Co_ma_cap_tren_thi_tra_muc_con_truc_tiep()
    {
        ThemPhuongCuaHaNoi();
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var body = await client.GetFromJsonAsync<JsonElement>($"{DanhMuc}?ma_dinh_danh=khu_vuc&ma_cap_tren=tinh_01");

        var item = body.EnumerateArray().Single().GetProperty("items").EnumerateArray().Single();
        Assert.Equal("px_10101003", item.GetProperty("ma").GetString());
        Assert.Equal(2, item.GetProperty("cap").GetInt32());
    }

    [Theory]
    [InlineData("?ma_cap_tren=tinh_01")]
    [InlineData("?ma_dinh_danh=khu_vuc,chuyen_mon_pt&ma_cap_tren=tinh_01")]
    public async Task Tra_400_khi_co_ma_cap_tren_ma_khong_dung_mot_loai(string query)
    {
        var client = TestApp.ClientWithRole(_app, AppRoles.User);

        var response = await client.GetAsync($"{DanhMuc}{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tra_cuu_ma_nhan_ca_muc_cap_2()
    {
        ThemPhuongCuaHaNoi();
        using var scope = _app.Services.CreateScope();
        var traCuu = scope.ServiceProvider.GetRequiredService<IDanhMucTraCuu>();

        Assert.True(await traCuu.BLKiemTraDangDungAsync("khu_vuc", "px_10101003", CancellationToken.None));
        Assert.False(await traCuu.BLKiemTraDangDungAsync("khu_vuc", "px_99999", CancellationToken.None));
    }

    private void ThemPhuongCuaHaNoi()
    {
        using var scope = _app.Services.CreateScope();
        // Dựng repository trước để dữ liệu tham chiếu được nạp vào store.
        scope.ServiceProvider.GetRequiredService<IDanhMucRepository>();
        var store = _app.Services.GetRequiredService<InMemoryStore<danh_muc>>();
        var haNoi = store.All().Single(e => e.ma_dinh_danh == "khu_vuc" && e.ma == "tinh_01");
        store.TryAdd(new danh_muc
        {
            ma_dinh_danh = "khu_vuc",
            ma = "px_10101003",
            ten = "Phường Ba Đình",
            cap = 2,
            danh_muc_cap_tren_id = haNoi.Id,
            tinh_trang = true,
        });
    }

    [Fact]
    public async Task Tra_401_khi_chua_dang_nhap()
    {
        var client = _app.GetTestClient();

        var response = await client.GetAsync(DanhMuc);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
