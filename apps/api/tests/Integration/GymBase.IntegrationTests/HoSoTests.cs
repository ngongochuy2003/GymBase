using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Base.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>
/// API hồ sơ sau onboarding: <c>GET/PUT /api/v1/mob/ho-so</c>. Một lần lưu ghi cả hồ sơ và chỉ số cơ thể (chỉ thêm
/// dòng <c>chi_so_co_the</c> khi số đo đổi). Tài khoản dùng lại id trong dữ liệu giả của <see cref="InMemoryTaiKhoanRepository"/>.
/// </summary>
public sealed class HoSoTests : IAsyncLifetime
{
    private const string HoSo = "/api/v1/mob/ho-so";

    /// <summary>hv.huy — Ngô Ngọc Huy, tài khoản đang hoạt động.</summary>
    private static readonly Guid HocVien = Guid.Parse("0193f0a0-0000-7000-8000-000000000004");

    /// <summary>pt.minh — Nguyễn Văn Minh, tài khoản đang hoạt động.</summary>
    private static readonly Guid Pt = Guid.Parse("0193f0a0-0000-7000-8000-000000000002");

    private WebApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApp.StartAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private HttpClient Client(Guid userId, string role = "User") => TestApp.ClientFor(_app, userId, role);

    private InMemoryChiSoCoTheStore ChiSoStore => _app.Services.GetRequiredService<InMemoryChiSoCoTheStore>();

    private static object BodyHocVien(
        string mucTieu = "tang_co",
        string khuVuc = "ha_noi",
        decimal chieuCao = 172.5m,
        decimal canNang = 68.4m,
        string loaiNguoiDung = "hoc_vien") => new
    {
        loai_nguoi_dung = loaiNguoiDung,
        muc_tieu_tap_luyen = mucTieu,
        chieu_cao_cm = chieuCao,
        can_nang_kg = canNang,
        khu_vuc_hoat_dong = khuVuc,
    };

    private static Task<HttpResponseMessage> Luu(HttpClient client, object body) => client.PutAsJsonAsync(HoSo, body);

    private static async Task<string?> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    [Fact]
    public async Task Luu_lan_dau_tao_ho_so_va_chi_so_co_the()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("hoc_vien", body.GetProperty("loai_nguoi_dung").GetString());

        // Tên hiển thị lấy từ tai_khoan.ho_ten; hồ sơ mới chưa xác minh.
        Assert.Equal("Ngô Ngọc Huy", body.GetProperty("ten_hien_thi").GetString());
        Assert.Equal("chua_xac_minh", body.GetProperty("trang_thai_xac_minh").GetString());
        Assert.Equal("tang_co", body.GetProperty("muc_tieu_tap_luyen").GetString());
        Assert.Equal("ha_noi", body.GetProperty("khu_vuc_hoat_dong").GetString());

        var chiSo = body.GetProperty("chi_so_co_the");
        Assert.Equal(172.5m, chiSo.GetProperty("chieu_cao_cm").GetDecimal());
        Assert.Equal(68.4m, chiSo.GetProperty("can_nang_kg").GetDecimal());
        Assert.Single(ChiSoStore.All());

        // GET sau khi lưu trả đúng hồ sơ vừa ghi.
        var doc = await client.GetFromJsonAsync<JsonElement>(HoSo);
        Assert.Equal("hoc_vien", doc.GetProperty("loai_nguoi_dung").GetString());
        Assert.Equal(68.4m, doc.GetProperty("chi_so_co_the").GetProperty("can_nang_kg").GetDecimal());
    }

    [Fact]
    public async Task Luu_lai_cung_so_do_khong_them_chi_so_moi()
    {
        var client = Client(HocVien);
        await Luu(client, BodyHocVien());

        var response = await Luu(client, BodyHocVien(mucTieu: "giam_mo"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(ChiSoStore.All());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("giam_mo", body.GetProperty("muc_tieu_tap_luyen").GetString());

        // Đổi một số đo thì thêm đúng một dòng mới, hồ sơ vẫn một dòng.
        await Luu(client, BodyHocVien(canNang: 67.9m));
        Assert.Equal(2, ChiSoStore.All().Count);
        var hoSoStore = _app.Services.GetRequiredService<InMemoryHoSoNguoiDungStore>();
        Assert.NotNull(hoSoStore.Find(HocVien));
    }

    [Fact]
    public async Task Luu_lai_giu_ten_hien_thi_va_trang_thai_xac_minh()
    {
        _app.Services.GetRequiredService<InMemoryHoSoNguoiDungStore>().Set(new ho_so_nguoi_dung
        {
            tai_khoan_id = HocVien,
            loai_nguoi_dung = LoaiNguoiDung.hoc_vien,
            ten_hien_thi = "Huy tự đặt",
            trang_thai_xac_minh = TrangThaiXacMinh.da_xac_minh,
            ngay_xac_minh = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc),
        });
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Huy tự đặt", body.GetProperty("ten_hien_thi").GetString());
        Assert.Equal("da_xac_minh", body.GetProperty("trang_thai_xac_minh").GetString());
    }

    [Fact]
    public async Task Pt_gui_chuyen_mon_hop_le_duoc_luu()
    {
        var client = Client(Pt);

        var response = await Luu(client, new
        {
            loai_nguoi_dung = "pt",
            muc_tieu_tap_luyen = "tang_co",
            chieu_cao_cm = 178m,
            can_nang_kg = 75m,
            khu_vuc_hoat_dong = "tp_hcm",
            chuyen_mon = new[] { "tang_co", "dinh_duong" },
            so_nam_kinh_nghiem = 4,
            gioi_thieu = "HLV 4 năm kinh nghiệm.",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("pt", body.GetProperty("loai_nguoi_dung").GetString());
        Assert.Equal(["tang_co", "dinh_duong"], body.GetProperty("chuyen_mon").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal(4, body.GetProperty("so_nam_kinh_nghiem").GetInt16());
    }

    [Fact]
    public async Task Ma_danh_muc_sai_tra_400_CATEGORY_INVALID()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(mucTieu: "khong_co_that"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CATEGORY_INVALID", await CodeOf(response));
        Assert.Empty(ChiSoStore.All());
    }

    [Fact]
    public async Task Chieu_cao_ngoai_khoang_tra_400_va_chi_ro_truong()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(chieuCao: 99m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("chieu_cao_cm", out _));
    }

    [Fact]
    public async Task Hoc_vien_gui_thong_tin_pt_tra_400_ROLE_INVALID()
    {
        var client = Client(HocVien);

        var response = await Luu(client, new
        {
            loai_nguoi_dung = "hoc_vien",
            muc_tieu_tap_luyen = "tang_co",
            chieu_cao_cm = 172.5m,
            can_nang_kg = 68.4m,
            khu_vuc_hoat_dong = "ha_noi",
            chuyen_mon = new[] { "tang_co" },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("ROLE_INVALID", await CodeOf(response));
    }

    [Fact]
    public async Task Vai_tro_khong_hop_le_tra_400_ROLE_INVALID()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(loaiNguoiDung: "admin"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("ROLE_INVALID", await CodeOf(response));
    }

    [Fact]
    public async Task Xem_ho_so_khi_chua_thiet_lap_tra_404_PROFILE_NOT_FOUND()
    {
        var client = Client(HocVien);

        var response = await client.GetAsync(HoSo);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("PROFILE_NOT_FOUND", await CodeOf(response));
    }

    [Fact]
    public async Task Tra_401_khi_chua_dang_nhap()
    {
        var client = _app.GetTestClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(HoSo)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Luu(client, BodyHocVien())).StatusCode);
    }
}
