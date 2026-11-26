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
        string khuVuc = "tinh_01",
        decimal chieuCao = 172.5m,
        decimal canNang = 68.4m,
        string loaiNguoiDung = "hoc_vien",
        string ngaySinh = "1998-05-20",
        string gioiTinh = "nam",
        string trinhDo = "co_nen_tang",
        string? tenHienThi = null) => new
    {
        loai_nguoi_dung = loaiNguoiDung,
        ten_hien_thi = tenHienThi,
        ngay_sinh = ngaySinh,
        gioi_tinh = gioiTinh,
        trinh_do_tap = trinhDo,
        muc_tieu_tap_luyen = mucTieu,
        chieu_cao_cm = chieuCao,
        can_nang_kg = canNang,
        khu_vuc_hoat_dong = khuVuc,
    };

    /// <summary>Body của PT chỉ có khối PT (không có khối học viên).</summary>
    private static Dictionary<string, object?> BodyPt(
        string loai = "pt",
        string[]? chuyenMon = null,
        int? soNam = 4,
        string? gioiThieu = "HLV 4 năm kinh nghiệm.") => new()
    {
        ["loai_nguoi_dung"] = loai,
        ["ngay_sinh"] = "1990-03-10",
        ["gioi_tinh"] = "nam",
        ["khu_vuc_hoat_dong"] = "tinh_29",
        ["chuyen_mon"] = chuyenMon ?? ["tang_co", "dinh_duong"],
        ["so_nam_kinh_nghiem"] = soNam,
        ["gioi_thieu"] = gioiThieu,
    };

    /// <summary>Body đủ cả hai khối, dùng cho ca_hai.</summary>
    private static Dictionary<string, object?> BodyCaHai()
    {
        var body = BodyPt(loai: "ca_hai");
        body["trinh_do_tap"] = "co_nen_tang";
        body["muc_tieu_tap_luyen"] = "tang_co";
        body["chieu_cao_cm"] = 178m;
        body["can_nang_kg"] = 75m;
        return body;
    }

    private static async Task AssertLoiTruongAsync(HttpResponseMessage response, string truong)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty(truong, out _), $"thiếu lỗi ở trường {truong}");
    }

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
        Assert.Equal("tinh_01", body.GetProperty("khu_vuc_hoat_dong").GetString());

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
            ngay_sinh = "1990-03-10",
            gioi_tinh = "nam",
            trinh_do_tap = "lau_nam",
            muc_tieu_tap_luyen = "tang_co",
            chieu_cao_cm = 178m,
            can_nang_kg = 75m,
            khu_vuc_hoat_dong = "tinh_29",
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
            ngay_sinh = "1998-05-20",
            gioi_tinh = "nam",
            trinh_do_tap = "co_nen_tang",
            muc_tieu_tap_luyen = "tang_co",
            chieu_cao_cm = 172.5m,
            can_nang_kg = 68.4m,
            khu_vuc_hoat_dong = "tinh_01",
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
    public async Task Luu_ngay_sinh_gioi_tinh_trinh_do_va_doc_lai_dung()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(ngaySinh: "1998-05-20", gioiTinh: "nu", trinhDo: "lau_nam"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await client.GetFromJsonAsync<JsonElement>(HoSo);
        Assert.Equal("1998-05-20", doc.GetProperty("ngay_sinh").GetString());
        Assert.Equal("nu", doc.GetProperty("gioi_tinh").GetString());
        Assert.Equal("lau_nam", doc.GetProperty("trinh_do_tap").GetString());
    }

    [Fact]
    public async Task Bo_ngay_sinh_gioi_tinh_khu_vuc_van_luu_duoc_va_doc_lai_null()
    {
        var client = Client(HocVien);
        var body = JsonSerializer.SerializeToNode(BodyHocVien())!.AsObject();
        body.Remove("ngay_sinh");
        body.Remove("gioi_tinh");
        body.Remove("khu_vuc_hoat_dong");

        var response = await Luu(client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await client.GetFromJsonAsync<JsonElement>(HoSo);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("ngay_sinh").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("gioi_tinh").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("khu_vuc_hoat_dong").ValueKind);
    }

    [Theory]
    [InlineData("trinh_do_tap")]
    public async Task Thieu_truong_ca_nhan_bat_buoc_tra_400(string truong)
    {
        var client = Client(HocVien);
        var body = JsonSerializer.SerializeToNode(BodyHocVien())!.AsObject();
        body.Remove(truong);

        var response = await Luu(client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty(truong, out _));
    }

    [Theory]
    [InlineData("2999-01-01")] // tương lai
    [InlineData("2020-01-01")] // dưới 13 tuổi
    [InlineData("1900-01-01")] // trên 100 tuổi
    public async Task Ngay_sinh_ngoai_khoang_tuoi_tra_400(string ngaySinh)
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(ngaySinh: ngaySinh));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("ngay_sinh", out _));
    }

    [Fact]
    public async Task Ma_trinh_do_sai_tra_400_CATEGORY_INVALID()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(trinhDo: "khong_co_that"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CATEGORY_INVALID", await CodeOf(response));
    }

    [Theory]
    [InlineData("nam")]
    [InlineData("nu")]
    [InlineData("khac")]
    [InlineData("khong_muon_noi")]
    public async Task Gioi_tinh_co_dinh_luu_va_doc_lai_dung(string gioiTinh)
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(gioiTinh: gioiTinh));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await client.GetFromJsonAsync<JsonElement>(HoSo);
        Assert.Equal(gioiTinh, doc.GetProperty("gioi_tinh").GetString());
    }

    [Theory]
    [InlineData("khong_co_that")]
    [InlineData("Nam")] // phân biệt hoa thường: app luôn gửi mã snake_case
    [InlineData("   ")]
    public async Task Gioi_tinh_ngoai_danh_sach_co_dinh_tra_400_va_chi_ro_truong(string gioiTinh)
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(gioiTinh: gioiTinh));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("gioi_tinh", out _));
    }

    [Fact]
    public async Task Gui_ten_hien_thi_thi_doi_khong_gui_thi_giu()
    {
        var client = Client(HocVien);

        var taoMoi = await (await Luu(client, BodyHocVien(tenHienThi: "  Huy Gym  "))).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Huy Gym", taoMoi.GetProperty("ten_hien_thi").GetString());

        var giu = await (await Luu(client, BodyHocVien())).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Huy Gym", giu.GetProperty("ten_hien_thi").GetString());

        var doi = await (await Luu(client, BodyHocVien(tenHienThi: "Huy Mới"))).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Huy Mới", doi.GetProperty("ten_hien_thi").GetString());
    }

    [Fact]
    public async Task Ten_hien_thi_rong_tra_400()
    {
        var client = Client(HocVien);

        var response = await Luu(client, BodyHocVien(tenHienThi: "   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty("ten_hien_thi", out _));
    }

    [Fact]
    public async Task Ho_so_cu_chua_co_truong_moi_doc_ra_null()
    {
        _app.Services.GetRequiredService<InMemoryHoSoNguoiDungStore>().Set(new ho_so_nguoi_dung
        {
            tai_khoan_id = HocVien,
            loai_nguoi_dung = LoaiNguoiDung.hoc_vien,
            ten_hien_thi = "Huy",
            trang_thai_xac_minh = TrangThaiXacMinh.chua_xac_minh,
        });
        var client = Client(HocVien);

        var doc = await client.GetFromJsonAsync<JsonElement>(HoSo);

        Assert.Equal(JsonValueKind.Null, doc.GetProperty("ngay_sinh").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("gioi_tinh").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("trinh_do_tap").ValueKind);
    }

    [Fact]
    public async Task Pt_chi_gui_khoi_pt_khong_can_khoi_hoc_vien()
    {
        var client = Client(Pt);

        var response = await Luu(client, BodyPt());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("pt", body.GetProperty("loai_nguoi_dung").GetString());
        Assert.Equal(["tang_co", "dinh_duong"], body.GetProperty("chuyen_mon").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal(4, body.GetProperty("so_nam_kinh_nghiem").GetInt16());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("muc_tieu_tap_luyen").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("trinh_do_tap").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("chi_so_co_the").ValueKind);
        Assert.Empty(ChiSoStore.All());
    }

    [Fact]
    public async Task Pt_thieu_chuyen_mon_tra_400()
    {
        var client = Client(Pt);
        var body = BodyPt();
        body["chuyen_mon"] = Array.Empty<string>();

        await AssertLoiTruongAsync(await Luu(client, body), "chuyen_mon");
    }

    [Fact]
    public async Task Pt_thieu_so_nam_kinh_nghiem_tra_400()
    {
        var client = Client(Pt);

        await AssertLoiTruongAsync(await Luu(client, BodyPt(soNam: null)), "so_nam_kinh_nghiem");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(61)]
    public async Task Pt_so_nam_kinh_nghiem_ngoai_khoang_tra_400(int soNam)
    {
        var client = Client(Pt);

        await AssertLoiTruongAsync(await Luu(client, BodyPt(soNam: soNam)), "so_nam_kinh_nghiem");
    }

    [Fact]
    public async Task Pt_gioi_thieu_qua_dai_tra_400()
    {
        var client = Client(Pt);

        await AssertLoiTruongAsync(await Luu(client, BodyPt(gioiThieu: new string('a', 1001))), "gioi_thieu");
    }

    [Fact]
    public async Task Pt_chuyen_mon_trung_duoc_gop_thanh_mot()
    {
        var client = Client(Pt);

        var response = await Luu(client, BodyPt(chuyenMon: ["tang_co", "tang_co", "giam_mo"]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(["tang_co", "giam_mo"], body.GetProperty("chuyen_mon").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task Pt_gui_chieu_cao_khong_kem_can_nang_tra_400()
    {
        var client = Client(Pt);
        var body = BodyPt();
        body["chieu_cao_cm"] = 178m;

        await AssertLoiTruongAsync(await Luu(client, body), "can_nang_kg");
    }

    [Fact]
    public async Task Ca_hai_gui_du_hai_khoi_duoc_luu()
    {
        var client = Client(Pt);

        var response = await Luu(client, BodyCaHai());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ca_hai", body.GetProperty("loai_nguoi_dung").GetString());
        Assert.Equal("tang_co", body.GetProperty("muc_tieu_tap_luyen").GetString());
        Assert.Equal(75m, body.GetProperty("chi_so_co_the").GetProperty("can_nang_kg").GetDecimal());
        Assert.Equal(4, body.GetProperty("so_nam_kinh_nghiem").GetInt16());
        Assert.Single(ChiSoStore.All());
    }

    [Fact]
    public async Task Ca_hai_thieu_khoi_pt_tra_400()
    {
        var client = Client(Pt);
        var body = BodyCaHai();
        body["chuyen_mon"] = null;

        await AssertLoiTruongAsync(await Luu(client, body), "chuyen_mon");
    }

    [Fact]
    public async Task Ca_hai_thieu_khoi_hoc_vien_tra_400()
    {
        var client = Client(Pt);
        var body = BodyCaHai();
        body.Remove("muc_tieu_tap_luyen");

        await AssertLoiTruongAsync(await Luu(client, body), "muc_tieu_tap_luyen");
    }

    [Fact]
    public async Task Hoc_vien_thieu_muc_tieu_tra_400()
    {
        var client = Client(HocVien);
        var body = JsonSerializer.SerializeToNode(BodyHocVien())!.AsObject();
        body.Remove("muc_tieu_tap_luyen");

        await AssertLoiTruongAsync(await Luu(client, body), "muc_tieu_tap_luyen");
    }

    [Fact]
    public async Task Doi_tu_pt_sang_hoc_vien_xoa_thong_tin_pt()
    {
        var client = Client(Pt);
        await Luu(client, BodyPt());

        var response = await Luu(client, BodyHocVien());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, body.GetProperty("chuyen_mon").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("so_nam_kinh_nghiem").ValueKind);
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

    // ---- Chứng chỉ PT (tệp PDF gắn vào hồ sơ qua tep_tin)

    private InMemoryStore<tep_tin> TepTinStore => _app.Services.GetRequiredService<InMemoryStore<tep_tin>>();

    private FakeFileStorage Storage => _app.Services.GetRequiredService<FakeFileStorage>();

    /// <summary>Tệp PDF chứng chỉ vừa tải lên (<c>da_tai_len</c>), chưa gắn vào hồ sơ.</summary>
    private Guid GivenChungChiDaTaiLen(Guid chuSoHuu, string mime = "application/pdf", string mucDich = "chung_chi_pt")
    {
        var file = new tep_tin
        {
            nguoi_tai_len_id = chuSoHuu,
            loai_doi_tuong = LoaiDoiTuongTepTin.ho_so_nguoi_dung,
            muc_dich = mucDich,
            s3_bucket = "FAKE",
            s3_key = $"chung_chi_pt/{Guid.NewGuid():N}.pdf",
            ten_tep_goc = "PT_Level1_FI.pdf",
            loai_mime = mime,
            kich_thuoc_byte = 2048,
            che_do_truy_cap = CheDoTruyCap.rieng_tu,
            trang_thai = TrangThaiTepTin.da_tai_len,
            ngay_tao = DateTime.UtcNow,
        };
        TepTinStore.TryAdd(file);
        return file.Id;
    }

    [Fact]
    public async Task Pt_gui_chung_chi_hop_le_duoc_gan_va_doc_lai()
    {
        var fileId = GivenChungChiDaTaiLen(Pt);
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = fileId;
        using var client = Client(Pt);

        var response = await Luu(client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var chungChi = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("chung_chi");
        Assert.Equal(fileId, chungChi.GetProperty("file_id").GetGuid());
        Assert.Equal("PT_Level1_FI.pdf", chungChi.GetProperty("ten_tep").GetString());
        Assert.Equal(2048, chungChi.GetProperty("kich_thuoc_byte").GetInt64());
        var luu = TepTinStore.Find(fileId)!;
        Assert.Equal(TrangThaiTepTin.da_lien_ket, luu.trang_thai);
        Assert.Equal(Pt, luu.doi_tuong_id);

        var doc = await (await client.GetAsync(HoSo)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(fileId, doc.GetProperty("chung_chi").GetProperty("file_id").GetGuid());
    }

    [Fact]
    public async Task Chung_chi_la_tuy_chon_va_ho_so_khong_co_thi_tra_null()
    {
        using var client = Client(Pt);

        var response = await Luu(client, BodyPt());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("chung_chi").ValueKind);
    }

    [Theory]
    [InlineData("application/pdf", "avatar")]
    [InlineData("image/webp", "chung_chi_pt")]
    public async Task Chung_chi_sai_muc_dich_hoac_sai_kieu_tra_400(string mime, string mucDich)
    {
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = GivenChungChiDaTaiLen(Pt, mime, mucDich);

        await AssertLoiTruongAsync(await Luu(Client(Pt), body), "chung_chi_tep_tin_id");
    }

    [Fact]
    public async Task Chung_chi_cua_nguoi_khac_tra_400_va_khong_bi_gan()
    {
        var cuaNguoiKhac = GivenChungChiDaTaiLen(HocVien);
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = cuaNguoiKhac;

        await AssertLoiTruongAsync(await Luu(Client(Pt), body), "chung_chi_tep_tin_id");

        Assert.Equal(TrangThaiTepTin.da_tai_len, TepTinStore.Find(cuaNguoiKhac)!.trang_thai);
    }

    [Fact]
    public async Task Chung_chi_khong_ton_tai_tra_400()
    {
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = Guid.NewGuid();

        await AssertLoiTruongAsync(await Luu(Client(Pt), body), "chung_chi_tep_tin_id");
    }

    [Fact]
    public async Task Hoc_vien_gui_chung_chi_tra_400_ROLE_INVALID()
    {
        var body = ToDictionary(BodyHocVien());
        body["chung_chi_tep_tin_id"] = GivenChungChiDaTaiLen(HocVien);

        var response = await Luu(Client(HocVien), body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("ROLE_INVALID", await CodeOf(response));
    }

    [Fact]
    public async Task Thay_chung_chi_moi_thi_chung_chi_cu_bi_go_va_xoa_khoi_kho()
    {
        using var client = Client(Pt);
        var cu = GivenChungChiDaTaiLen(Pt);
        var bodyCu = BodyPt();
        bodyCu["chung_chi_tep_tin_id"] = cu;
        await Luu(client, bodyCu);
        var moi = GivenChungChiDaTaiLen(Pt);
        var bodyMoi = BodyPt();
        bodyMoi["chung_chi_tep_tin_id"] = moi;

        var response = await Luu(client, bodyMoi);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TrangThaiTepTin.da_xoa, TepTinStore.Find(cu)!.trang_thai);
        Assert.Equal(TrangThaiTepTin.da_lien_ket, TepTinStore.Find(moi)!.trang_thai);
        Assert.Contains(TepTinStore.Find(cu)!.s3_key, Storage.Deleted);
    }

    [Fact]
    public async Task Luu_khong_gui_chung_chi_thi_go_chung_chi_dang_co()
    {
        using var client = Client(Pt);
        var fileId = GivenChungChiDaTaiLen(Pt);
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = fileId;
        await Luu(client, body);

        var response = await Luu(client, BodyPt());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("chung_chi").ValueKind);
        Assert.Equal(TrangThaiTepTin.da_xoa, TepTinStore.Find(fileId)!.trang_thai);
    }

    [Fact]
    public async Task Gui_lai_cung_chung_chi_thi_giu_nguyen_khong_xoa()
    {
        using var client = Client(Pt);
        var fileId = GivenChungChiDaTaiLen(Pt);
        var body = BodyPt();
        body["chung_chi_tep_tin_id"] = fileId;
        await Luu(client, body);

        var response = await Luu(client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TrangThaiTepTin.da_lien_ket, TepTinStore.Find(fileId)!.trang_thai);
        Assert.Empty(Storage.Deleted);
    }

    // ---- Ảnh đại diện

    private Guid GivenAnhDaiDienDaTaiLen(Guid chuSoHuu)
    {
        var id = GivenChungChiDaTaiLen(chuSoHuu, "image/jpeg", "avatar");
        TepTinStore.Find(id)!.loai_doi_tuong = null;
        return id;
    }

    [Fact]
    public async Task Gui_anh_dai_dien_thi_gan_anh_moi_va_xoa_anh_cu()
    {
        using var client = Client(Pt);
        var cu = GivenAnhDaiDienDaTaiLen(Pt);
        var body = BodyPt();
        body["anh_dai_dien_tep_tin_id"] = cu;
        Assert.Equal(HttpStatusCode.OK, (await Luu(client, body)).StatusCode);
        var moi = GivenAnhDaiDienDaTaiLen(Pt);
        body["anh_dai_dien_tep_tin_id"] = moi;

        var response = await Luu(client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TrangThaiTepTin.da_lien_ket, TepTinStore.Find(moi)!.trang_thai);
        Assert.Equal(TrangThaiTepTin.da_xoa, TepTinStore.Find(cu)!.trang_thai);
    }

    [Fact]
    public async Task Khong_gui_anh_dai_dien_thi_giu_anh_dang_co()
    {
        using var client = Client(Pt);
        var anh = GivenAnhDaiDienDaTaiLen(Pt);
        var body = BodyPt();
        body["anh_dai_dien_tep_tin_id"] = anh;
        await Luu(client, body);

        var response = await Luu(client, BodyPt());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TrangThaiTepTin.da_lien_ket, TepTinStore.Find(anh)!.trang_thai);
        Assert.Empty(Storage.Deleted);
    }

    [Fact]
    public async Task Anh_dai_dien_cua_nguoi_khac_tra_400()
    {
        using var client = Client(Pt);
        var body = BodyPt();
        body["anh_dai_dien_tep_tin_id"] = GivenAnhDaiDienDaTaiLen(HocVien);

        await AssertLoiTruongAsync(await Luu(client, body), "anh_dai_dien_tep_tin_id");
    }

    private static Dictionary<string, object?> ToDictionary(object body) =>
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(System.Text.Json.JsonSerializer.Serialize(body))!;
}
