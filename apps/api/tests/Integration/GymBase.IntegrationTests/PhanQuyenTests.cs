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
/// Phân quyền qua HTTP: policy theo hồ sơ (đọc database mỗi request, không dùng claim cũ trong token) và quyền trên dữ liệu
/// học viên. Thiết kế: docs/api/phan-quyen.md. Tài khoản dùng lại các id trong dữ liệu giả của InMemoryTaiKhoanRepository.
/// </summary>
public sealed class PhanQuyenTests : IAsyncLifetime
{
    private const string Prefix = "/api/v1/web/phan-quyen";

    /// <summary>pt.minh: đang hoạt động.</summary>
    private static readonly Guid PtHoatDong = Guid.Parse("0193f0a0-0000-7000-8000-000000000002");

    /// <summary>hv.phuong: tài khoản đã bị khóa (dang_hoat_dong = false).</summary>
    private static readonly Guid TaiKhoanBiKhoa = Guid.Parse("0193f0a0-0000-7000-8000-000000000005");

    private static readonly Guid HocVienH = Guid.Parse("0193f0a0-0000-7000-8000-000000000004");
    private static readonly Guid HocVienK = Guid.CreateVersion7();

    private WebApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApp.StartAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private HttpClient Client(Guid userId, string role = "User") => TestApp.ClientFor(_app, userId, role);

    private void GivenHoSo(Guid taiKhoanId, LoaiNguoiDung loai, TrangThaiXacMinh xacMinh) =>
        _app.Services.GetRequiredService<InMemoryHoSoNguoiDungStore>().Set(new ho_so_nguoi_dung
        {
            tai_khoan_id = taiKhoanId,
            loai_nguoi_dung = loai,
            ten_hien_thi = "Người dùng thử",
            trang_thai_xac_minh = xacMinh,
        });

    private void GivenKetNoi(Guid pt, Guid hocVien, TrangThaiKetNoiPt trangThai, params PhamViPhuTrach[] phamVi) =>
        _app.Services.GetRequiredService<InMemoryStore<ket_noi_pt>>().TryAdd(new ket_noi_pt
        {
            pt_id = pt,
            hoc_vien_id = hocVien,
            nguoi_khoi_tao_id = hocVien,
            pham_vi_phu_trach = [.. phamVi],
            trang_thai = trangThai,
            ngay_de_nghi = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
        });

    private static async Task<string?> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    // ---- Chưa đăng nhập

    [Theory]
    [InlineData("admin")]
    [InlineData("ho-so")]
    [InlineData("pt")]
    [InlineData("pt-xac-minh")]
    public async Task Anonymous_gets_401(string path)
    {
        using var client = _app.GetTestClient();

        var response = await client.GetAsync($"{Prefix}/{path}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---- Vai trò hệ thống

    [Fact]
    public async Task Admin_policy_needs_admin_role()
    {
        using var user = Client(PtHoatDong, "User");
        using var admin = Client(Guid.CreateVersion7(), AppRoles.Admin);

        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync($"{Prefix}/admin")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"{Prefix}/admin")).StatusCode);
    }

    [Fact]
    public async Task Admin_role_does_not_grant_pt_capability()
    {
        using var admin = Client(Guid.CreateVersion7(), AppRoles.Admin);

        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync($"{Prefix}/ho-so")).StatusCode);
    }

    // ---- Policy theo hồ sơ

    [Fact]
    public async Task Profile_policy_needs_a_profile()
    {
        var chuaChonNhom = Guid.CreateVersion7();
        var daChonNhom = Guid.CreateVersion7();
        GivenHoSo(daChonNhom, LoaiNguoiDung.hoc_vien, TrangThaiXacMinh.chua_xac_minh);
        using var khong = Client(chuaChonNhom);
        using var co = Client(daChonNhom);

        Assert.Equal(HttpStatusCode.Forbidden, (await khong.GetAsync($"{Prefix}/ho-so")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await co.GetAsync($"{Prefix}/ho-so")).StatusCode);
    }

    [Fact]
    public async Task Pt_profile_policy_accepts_unverified_pt_but_not_student()
    {
        var hocVien = Guid.CreateVersion7();
        var pt = Guid.CreateVersion7();
        GivenHoSo(hocVien, LoaiNguoiDung.hoc_vien, TrangThaiXacMinh.da_xac_minh);
        GivenHoSo(pt, LoaiNguoiDung.pt, TrangThaiXacMinh.chua_xac_minh);
        using var hocVienClient = Client(hocVien);
        using var ptClient = Client(pt);

        Assert.Equal(HttpStatusCode.Forbidden, (await hocVienClient.GetAsync($"{Prefix}/pt")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await ptClient.GetAsync($"{Prefix}/pt")).StatusCode);
    }

    [Theory]
    [InlineData(TrangThaiXacMinh.chua_xac_minh, HttpStatusCode.Forbidden)]
    [InlineData(TrangThaiXacMinh.dang_xem_xet, HttpStatusCode.Forbidden)]
    [InlineData(TrangThaiXacMinh.tu_choi, HttpStatusCode.Forbidden)]
    [InlineData(TrangThaiXacMinh.da_xac_minh, HttpStatusCode.OK)]
    public async Task Verified_pt_policy_follows_verification_status(TrangThaiXacMinh xacMinh, HttpStatusCode expected)
    {
        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, xacMinh);
        using var client = Client(PtHoatDong);

        Assert.Equal(expected, (await client.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);
    }

    [Fact]
    public async Task Locked_account_loses_pt_capability_with_the_same_token()
    {
        GivenHoSo(TaiKhoanBiKhoa, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);
        using var client = Client(TaiKhoanBiKhoa);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);
    }

    [Fact]
    public async Task Permission_changes_take_effect_on_next_request_without_new_token()
    {
        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, TrangThaiXacMinh.dang_xem_xet);
        using var client = Client(PtHoatDong);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);

        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);

        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, TrangThaiXacMinh.tu_choi);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"{Prefix}/pt-xac-minh")).StatusCode);
    }

    // ---- Quyền trên dữ liệu học viên (IDOR)

    [Fact]
    public async Task Pt_reaches_only_own_active_student_by_id()
    {
        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);
        GivenKetNoi(PtHoatDong, HocVienH, TrangThaiKetNoiPt.dang_hoat_dong, PhamViPhuTrach.dinh_duong);
        using var client = Client(PtHoatDong);

        var own = await client.GetAsync($"{Prefix}/hoc-vien/{HocVienH}/dinh-duong");
        var other = await client.GetAsync($"{Prefix}/hoc-vien/{HocVienK}/dinh-duong");

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);
        Assert.Equal("CONNECTION_NOT_ACTIVE", await CodeOf(other));
    }

    [Fact]
    public async Task Pt_is_rejected_outside_assigned_scope_with_specific_code()
    {
        GivenHoSo(PtHoatDong, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);
        GivenKetNoi(PtHoatDong, HocVienH, TrangThaiKetNoiPt.dang_hoat_dong, PhamViPhuTrach.tap_luyen);
        using var client = Client(PtHoatDong);

        var response = await client.GetAsync($"{Prefix}/hoc-vien/{HocVienH}/dinh-duong");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("SCOPE_NOT_ALLOWED", await CodeOf(response));
    }

    // ---- Endpoint cho client

    [Fact]
    public async Task Quyen_han_endpoint_reports_current_permission_for_both_channels()
    {
        GivenHoSo(PtHoatDong, LoaiNguoiDung.ca_hai, TrangThaiXacMinh.da_xac_minh);
        using var client = Client(PtHoatDong);

        foreach (var channel in new[] { "web", "mob" })
        {
            var response = await client.GetAsync($"/api/v1/{channel}/auth/quyen-han");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.GetProperty("da_thiet_lap_ho_so").GetBoolean());
            Assert.Equal("ca_hai", json.GetProperty("loai_nguoi_dung").GetString());
            Assert.True(json.GetProperty("la_pt_da_xac_minh").GetBoolean());
            Assert.Equal(NangLuc.QuanLyHocVien, json.GetProperty("nang_luc")[0].GetString());
        }
    }

    [Fact]
    public async Task Quyen_han_endpoint_requires_login()
    {
        using var client = _app.GetTestClient();

        var response = await client.GetAsync("/api/v1/web/auth/quyen-han");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

}
