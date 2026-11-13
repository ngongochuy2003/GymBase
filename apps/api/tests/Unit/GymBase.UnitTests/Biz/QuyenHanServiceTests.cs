using GymBase.Base.Auth;
using GymBase.BLBiz.BLQuyenHan;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Biz;

/// <summary>
/// Quyền theo hồ sơ: chỉ PT có hồ sơ đã xác minh và tài khoản không bị khóa mới được quản lý học viên.
/// Mỗi dòng của ma trận trong docs/api/phan-quyen.md là một test ở đây.
/// </summary>
public class QuyenHanServiceTests
{
    private static readonly Guid TaiKhoanId = Guid.CreateVersion7();

    private readonly InMemoryHoSoNguoiDungStore _store = new();

    private sealed class FakeTrangThai(bool hoatDong) : ITaiKhoanTrangThaiProvider
    {
        public Task<bool> BLKiemTraHoatDongAsync(Guid taiKhoanId, CancellationToken cancellationToken) => Task.FromResult(hoatDong);
    }

    private QuyenHanService CreateService(bool taiKhoanHoatDong = true) =>
        new(new InMemoryHoSoNguoiDungRepository(_store), new FakeTrangThai(taiKhoanHoatDong));

    private void GivenHoSo(LoaiNguoiDung loai, TrangThaiXacMinh xacMinh) =>
        _store.Set(new ho_so_nguoi_dung
        {
            tai_khoan_id = TaiKhoanId,
            loai_nguoi_dung = loai,
            ten_hien_thi = "Người dùng thử",
            trang_thai_xac_minh = xacMinh,
        });

    [Fact]
    public async Task User_without_profile_has_no_permission()
    {
        var quyen = await CreateService().BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None);

        Assert.False(quyen.da_thiet_lap_ho_so);
        Assert.False(quyen.la_pt);
        Assert.False(quyen.la_pt_da_xac_minh);
        Assert.Null(quyen.loai_nguoi_dung);
        Assert.Empty(quyen.nang_luc);
    }

    [Theory]
    [InlineData(TrangThaiXacMinh.chua_xac_minh)]
    [InlineData(TrangThaiXacMinh.da_xac_minh)]
    public async Task Hoc_vien_is_never_a_pt_even_if_marked_verified(TrangThaiXacMinh xacMinh)
    {
        GivenHoSo(LoaiNguoiDung.hoc_vien, xacMinh);

        var quyen = await CreateService().BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None);

        Assert.True(quyen.da_thiet_lap_ho_so);
        Assert.False(quyen.la_pt);
        Assert.False(quyen.la_pt_da_xac_minh);
        Assert.Empty(quyen.nang_luc);
    }

    [Theory]
    [InlineData(LoaiNguoiDung.pt, TrangThaiXacMinh.chua_xac_minh)]
    [InlineData(LoaiNguoiDung.pt, TrangThaiXacMinh.dang_xem_xet)]
    [InlineData(LoaiNguoiDung.pt, TrangThaiXacMinh.tu_choi)]
    [InlineData(LoaiNguoiDung.ca_hai, TrangThaiXacMinh.chua_xac_minh)]
    public async Task Unverified_pt_has_pt_profile_but_cannot_manage_students(LoaiNguoiDung loai, TrangThaiXacMinh xacMinh)
    {
        GivenHoSo(loai, xacMinh);

        var quyen = await CreateService().BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None);

        Assert.True(quyen.la_pt);
        Assert.False(quyen.la_pt_da_xac_minh);
        Assert.Empty(quyen.nang_luc);
        Assert.Equal(xacMinh.ToString(), quyen.trang_thai_xac_minh);
    }

    [Theory]
    [InlineData(LoaiNguoiDung.pt)]
    [InlineData(LoaiNguoiDung.ca_hai)]
    public async Task Verified_pt_can_manage_students(LoaiNguoiDung loai)
    {
        GivenHoSo(loai, TrangThaiXacMinh.da_xac_minh);

        var quyen = await CreateService().BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None);

        Assert.True(quyen.la_pt);
        Assert.True(quyen.la_pt_da_xac_minh);
        Assert.Equal([NangLuc.QuanLyHocVien], quyen.nang_luc);
        Assert.Equal(loai.ToString(), quyen.loai_nguoi_dung);
    }

    [Fact]
    public async Task Verified_pt_loses_capability_when_account_is_locked()
    {
        GivenHoSo(LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);

        var quyen = await CreateService(taiKhoanHoatDong: false).BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None);

        Assert.True(quyen.la_pt);
        Assert.False(quyen.la_pt_da_xac_minh);
        Assert.Empty(quyen.nang_luc);
    }

    [Fact]
    public async Task Permission_follows_database_and_not_a_cached_value()
    {
        GivenHoSo(LoaiNguoiDung.pt, TrangThaiXacMinh.dang_xem_xet);
        var service = CreateService();
        Assert.False((await service.BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None)).la_pt_da_xac_minh);

        GivenHoSo(LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh);

        Assert.True((await service.BLLayQuyenHanAsync(TaiKhoanId, CancellationToken.None)).la_pt_da_xac_minh);
    }
}
