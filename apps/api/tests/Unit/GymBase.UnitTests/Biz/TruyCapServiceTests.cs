using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.BLBiz;
using GymBase.BLBiz.BLQuyenHan;
using GymBase.BLBiz.BLTruyCap;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Biz;

/// <summary>
/// Quyền trên từng bản ghi: PT chỉ động tới dữ liệu của học viên có kết nối với mình, đúng lĩnh vực. Có các ca IDOR:
/// id kết nối hoặc id học viên lấy từ request nhưng không khớp với dữ liệu trong DB.
/// </summary>
public class TruyCapServiceTests
{
    private readonly Guid _ptA = Guid.CreateVersion7();
    private readonly Guid _ptB = Guid.CreateVersion7();
    private readonly Guid _hocVienH = Guid.CreateVersion7();
    private readonly Guid _hocVienK = Guid.CreateVersion7();
    private readonly Guid _ketNoiAH = Guid.CreateVersion7();
    private readonly Guid _ketNoiAK = Guid.CreateVersion7();
    private readonly Guid _ketNoiBH = Guid.CreateVersion7();

    private readonly InMemoryHoSoNguoiDungStore _hoSos = new();
    private readonly InMemoryStore<ket_noi_pt> _ketNois = new();
    private readonly HashSet<Guid> _taiKhoanBiKhoa = [];

    private sealed class FakeTrangThai(HashSet<Guid> biKhoa) : ITaiKhoanTrangThaiProvider
    {
        public Task<bool> BLKiemTraHoatDongAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
            Task.FromResult(!biKhoa.Contains(taiKhoanId));
    }

    public TruyCapServiceTests()
    {
        _hoSos.Set(HoSo(_ptA, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh));
        _hoSos.Set(HoSo(_ptB, LoaiNguoiDung.pt, TrangThaiXacMinh.da_xac_minh));
        _hoSos.Set(HoSo(_hocVienH, LoaiNguoiDung.hoc_vien, TrangThaiXacMinh.chua_xac_minh));
        _hoSos.Set(HoSo(_hocVienK, LoaiNguoiDung.hoc_vien, TrangThaiXacMinh.chua_xac_minh));

        // A phụ trách dinh_duong của H (đang hoạt động) và đã kết thúc với K; B mới gửi lời mời cho H, chưa được chấp nhận.
        _ketNois.TryAdd(KetNoi(_ketNoiAH, _ptA, _hocVienH, TrangThaiKetNoiPt.dang_hoat_dong, PhamViPhuTrach.dinh_duong));
        _ketNois.TryAdd(KetNoi(_ketNoiAK, _ptA, _hocVienK, TrangThaiKetNoiPt.da_ket_thuc, PhamViPhuTrach.dinh_duong, PhamViPhuTrach.tap_luyen));
        _ketNois.TryAdd(KetNoi(_ketNoiBH, _ptB, _hocVienH, TrangThaiKetNoiPt.cho_phan_hoi, PhamViPhuTrach.tap_luyen));
    }

    private TruyCapService CreateService()
    {
        var quyenHan = new QuyenHanService(new InMemoryHoSoNguoiDungRepository(_hoSos), new FakeTrangThai(_taiKhoanBiKhoa));
        return new TruyCapService(quyenHan, new InMemoryKetNoiPtRepository(_ketNois));
    }

    private static ho_so_nguoi_dung HoSo(Guid taiKhoanId, LoaiNguoiDung loai, TrangThaiXacMinh xacMinh) => new()
    {
        tai_khoan_id = taiKhoanId,
        loai_nguoi_dung = loai,
        ten_hien_thi = "Người dùng thử",
        trang_thai_xac_minh = xacMinh,
    };

    private static ket_noi_pt KetNoi(Guid id, Guid pt, Guid hocVien, TrangThaiKetNoiPt trangThai, params PhamViPhuTrach[] phamVi) => new()
    {
        Id = id,
        pt_id = pt,
        hoc_vien_id = hocVien,
        nguoi_khoi_tao_id = hocVien,
        pham_vi_phu_trach = [.. phamVi],
        trang_thai = trangThai,
        ngay_de_nghi = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    private static async Task AssertForbidden(string code, Func<Task> act) =>
        Assert.Equal(code, (await Assert.ThrowsAsync<ForbiddenException>(act)).Code);

    // ---- Quản lý học viên (ghi)

    [Fact]
    public async Task Pt_manages_own_active_student_in_assigned_scope()
    {
        await CreateService().BLKiemTraQuanLyHocVienAsync(_ptA, _hocVienH, PhamViPhuTrach.dinh_duong, CancellationToken.None);
    }

    [Fact]
    public async Task Pt_cannot_manage_scope_outside_connection()
    {
        await AssertForbidden(BizErrorCodes.ScopeNotAllowed, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_ptA, _hocVienH, PhamViPhuTrach.tap_luyen, CancellationToken.None));
    }

    [Fact]
    public async Task Pt_cannot_manage_student_of_another_pt()
    {
        await AssertForbidden(BizErrorCodes.ConnectionNotActive, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_ptB, _hocVienH, PhamViPhuTrach.tap_luyen, CancellationToken.None));
    }

    [Fact]
    public async Task Pt_cannot_manage_student_after_connection_ended()
    {
        await AssertForbidden(BizErrorCodes.ConnectionNotActive, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_ptA, _hocVienK, PhamViPhuTrach.dinh_duong, CancellationToken.None));
    }

    [Fact]
    public async Task Unverified_pt_cannot_manage_even_with_active_connection()
    {
        _hoSos.Set(HoSo(_ptA, LoaiNguoiDung.pt, TrangThaiXacMinh.dang_xem_xet));

        await AssertForbidden(BizErrorCodes.PtNotVerified, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_ptA, _hocVienH, PhamViPhuTrach.dinh_duong, CancellationToken.None));
    }

    [Fact]
    public async Task Locked_pt_cannot_manage()
    {
        _taiKhoanBiKhoa.Add(_ptA);

        await AssertForbidden(BizErrorCodes.PtNotVerified, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_ptA, _hocVienH, PhamViPhuTrach.dinh_duong, CancellationToken.None));
    }

    [Fact]
    public async Task Student_cannot_manage_own_data_as_pt()
    {
        await AssertForbidden(BizErrorCodes.PtNotVerified, () =>
            CreateService().BLKiemTraQuanLyHocVienAsync(_hocVienH, _hocVienH, PhamViPhuTrach.dinh_duong, CancellationToken.None));
    }

    // ---- Đọc dữ liệu học viên

    [Fact]
    public async Task Owner_reads_own_data_even_without_connection()
    {
        await CreateService().BLKiemTraDocDuLieuHocVienAsync(_hocVienH, _hocVienH, null, CancellationToken.None);
    }

    [Fact]
    public async Task Pt_cannot_read_private_data_without_connection_id()
    {
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienH, null, CancellationToken.None));
    }

    [Fact]
    public async Task Pt_reads_data_shared_through_own_active_connection()
    {
        await CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienH, _ketNoiAH, CancellationToken.None);
    }

    [Fact]
    public async Task Pt_reads_history_after_connection_ended()
    {
        await CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienK, _ketNoiAK, CancellationToken.None);
    }

    [Fact]
    public async Task Idor_connection_of_another_student_is_rejected()
    {
        // A có thật một kết nối hợp lệ (với K), nhưng dùng id đó để đọc dữ liệu của H.
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienH, _ketNoiAK, CancellationToken.None));
    }

    [Fact]
    public async Task Idor_connection_of_another_pt_is_rejected()
    {
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptB, _hocVienH, _ketNoiAH, CancellationToken.None));
    }

    [Fact]
    public async Task Unknown_connection_id_is_rejected()
    {
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienH, Guid.CreateVersion7(), CancellationToken.None));
    }

    [Fact]
    public async Task Pending_connection_shares_nothing()
    {
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptB, _hocVienH, _ketNoiBH, CancellationToken.None));
    }

    [Fact]
    public async Task Pt_who_lost_verification_cannot_read_active_connection_data()
    {
        _hoSos.Set(HoSo(_ptA, LoaiNguoiDung.pt, TrangThaiXacMinh.tu_choi));

        await AssertForbidden(BizErrorCodes.PtNotVerified, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_ptA, _hocVienH, _ketNoiAH, CancellationToken.None));
    }

    [Fact]
    public async Task Another_student_cannot_read_data()
    {
        await AssertForbidden(BizErrorCodes.DataNotShared, () =>
            CreateService().BLKiemTraDocDuLieuHocVienAsync(_hocVienK, _hocVienH, _ketNoiAH, CancellationToken.None));
    }
}
