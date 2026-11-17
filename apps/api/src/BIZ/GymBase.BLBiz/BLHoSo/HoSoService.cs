using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.DanhMuc;

namespace GymBase.BLBiz.BLHoSo;

/// <summary>
/// Hồ sơ người dùng sau onboarding: xem hồ sơ hiện tại và lưu một lần cả hồ sơ lẫn chỉ số cơ thể.
/// Mã danh mục được kiểm tra qua <see cref="IDanhMucTraCuu"/> (không tham chiếu BLDI).
/// </summary>
public sealed class HoSoService(
    IHoSoNguoiDungRepository hoSoRepository,
    IChiSoCoTheRepository chiSoRepository,
    IDanhMucTraCuu danhMucTraCuu,
    ITaiKhoanHoTenProvider hoTenProvider,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    private const string MucTieuTapLuyen = "muc_tieu_tap_luyen";
    private const string KhuVuc = "khu_vuc";
    private const string ChuyenMonPt = "chuyen_mon_pt";

    private const int ChuyenMonToiDa = 10;
    private const decimal ChieuCaoNhoNhat = 100;
    private const decimal ChieuCaoLonNhat = 250;
    private const decimal CanNangNhoNhat = 20;
    private const decimal CanNangLonNhat = 300;
    private const short KinhNghiemLonNhat = 60;
    private const int GioiThieuToiDa = 1000;
    private const int TenHienThiToiDa = 100;
    private const int TuoiNhoNhat = 13;
    private const int TuoiLonNhat = 100;
    private const string TrinhDoTap = "trinh_do_tap";

    /// <summary>Hồ sơ hiện tại kèm chỉ số cơ thể mới nhất.</summary>
    /// <exception cref="NotFoundException"><see cref="BizErrorCodes.ProfileNotFound"/>: tài khoản chưa thiết lập hồ sơ.</exception>
    public async Task<HoSoDto> BLXemHoSoAsync(CancellationToken cancellationToken)
    {
        var taiKhoanId = TaiKhoanIdHienTai();
        var hoSo = await hoSoRepository.DLFindByTaiKhoanIdAsync(taiKhoanId, cancellationToken)
            ?? throw new NotFoundException(
                "Bạn chưa thiết lập hồ sơ. Hãy thiết lập hồ sơ trước.", BizErrorCodes.ProfileNotFound);
        var chiSo = await chiSoRepository.DLFindMoiNhatAsync(taiKhoanId, cancellationToken);
        return ToDto(hoSo, chiSo);
    }

    /// <summary>
    /// Lưu hồ sơ và chỉ số cơ thể trong một transaction. Chỉ thêm dòng <c>chi_so_co_the</c> khi chiều cao hoặc cân nặng
    /// khác bản ghi mới nhất. Không đổi <c>ten_hien_thi</c>, ảnh đại diện và trạng thái xác minh của hồ sơ đã có.
    /// </summary>
    public async Task<HoSoDto> BLLuuHoSoAsync(HoSoLuuDto dto, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var loaiNguoiDung = DocLoaiNguoiDung(dto);
        var gioiTinh = DocGioiTinh(dto);
        var tenHienThi = ChuanHoaTenHienThi(dto.ten_hien_thi);
        await KiemTraDanhMucAsync(dto, loaiNguoiDung, DateOnly.FromDateTime(now), cancellationToken);

        var taiKhoanId = TaiKhoanIdHienTai();

        // AsNoTracking: entity trả về được sửa rồi ghi lại trong một lần ở DL.
        var hoSo = await hoSoRepository.DLFindByTaiKhoanIdAsync(taiKhoanId, cancellationToken);
        if (hoSo is null)
        {
            hoSo = new ho_so_nguoi_dung
            {
                tai_khoan_id = taiKhoanId,
                ten_hien_thi = tenHienThi
                    ?? await hoTenProvider.BLDocHoTenAsync(taiKhoanId, cancellationToken)
                    ?? currentUser.UserName
                    ?? string.Empty,
                trang_thai_xac_minh = TrangThaiXacMinh.chua_xac_minh,

                // Bảng này khai báo bốn cột audit bằng tay (không kế thừa BaseAuditableEntity) nên tự gán.
                ngay_tao = now,
                nguoi_tao = currentUser.UserName,
            };
        }
        else
        {
            hoSo.ngay_chinh_sua = now;
            hoSo.nguoi_chinh_sua = currentUser.UserName;
            if (tenHienThi is not null)
            {
                hoSo.ten_hien_thi = tenHienThi;
            }
        }

        hoSo.loai_nguoi_dung = loaiNguoiDung;
        hoSo.ngay_sinh = dto.ngay_sinh;
        hoSo.gioi_tinh = gioiTinh;
        hoSo.trinh_do_tap = ChuanHoaChuoi(dto.trinh_do_tap);
        hoSo.muc_tieu_tap_luyen = ChuanHoaChuoi(dto.muc_tieu_tap_luyen);
        hoSo.khu_vuc_hoat_dong = ChuanHoaChuoi(dto.khu_vuc_hoat_dong);
        hoSo.chuyen_mon = ChuanHoaChuyenMon(dto.chuyen_mon);
        hoSo.so_nam_kinh_nghiem = dto.so_nam_kinh_nghiem;
        hoSo.gioi_thieu = ChuanHoaChuoi(dto.gioi_thieu);

        var moiNhat = await chiSoRepository.DLFindMoiNhatAsync(taiKhoanId, cancellationToken);

        // Chỉ ghi chỉ số cơ thể khi có số đo (PT không gửi khối học viên thì bỏ qua) và số đo khác bản ghi mới nhất.
        chi_so_co_the? chiSoMoi = null;
        if (dto.chieu_cao_cm is { } chieuCaoGui && dto.can_nang_kg is { } canNangGui)
        {
            var chieuCao = Math.Round(chieuCaoGui, 1);
            var canNang = Math.Round(canNangGui, 2);
            if (moiNhat is null || moiNhat.chieu_cao_cm != chieuCao || moiNhat.can_nang_kg != canNang)
            {
                chiSoMoi = new chi_so_co_the
                {
                    hoc_vien_id = taiKhoanId,
                    nguoi_nhap_id = taiKhoanId,
                    ngay_do = now.Date,
                    chieu_cao_cm = chieuCao,
                    can_nang_kg = canNang,
                    ngay_tao = now,
                };
            }
        }

        await hoSoRepository.DLUpsertAsync(hoSo, chiSoMoi, cancellationToken);
        return ToDto(hoSo, chiSoMoi ?? moiNhat);
    }

    private Guid TaiKhoanIdHienTai() => currentUser.UserId ?? throw new UnauthorizedException();

    private static LoaiNguoiDung DocLoaiNguoiDung(HoSoLuuDto dto)
    {
        var giaTri = dto.loai_nguoi_dung.Trim();
        return giaTri switch
        {
            nameof(LoaiNguoiDung.hoc_vien) => LoaiNguoiDung.hoc_vien,
            nameof(LoaiNguoiDung.pt) => LoaiNguoiDung.pt,
            nameof(LoaiNguoiDung.ca_hai) => LoaiNguoiDung.ca_hai,
            _ => throw new ValidationFailedException(
                nameof(HoSoLuuDto.loai_nguoi_dung),
                "Vai trò sử dụng không hợp lệ.",
                BizErrorCodes.RoleInvalid),
        };
    }

    /// <summary>Tên hiển thị đã cắt khoảng trắng; null nếu không gửi. Gửi chuỗi rỗng hoặc quá dài là lỗi.</summary>
    private static string? ChuanHoaTenHienThi(string? tenHienThi)
    {
        if (tenHienThi is null)
        {
            return null;
        }

        var ten = tenHienThi.Trim();
        if (ten.Length is 0 or > TenHienThiToiDa)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.ten_hien_thi), $"Tên hiển thị phải từ 1 đến {TenHienThiToiDa} ký tự.");
        }

        return ten;
    }

    private static void KiemTraNgaySinh(DateOnly? ngaySinh, DateOnly homNay)
    {
        // Ngày sinh tùy chọn: không gửi thì bỏ qua.
        if (ngaySinh is not { } ngay)
        {
            return;
        }

        var tuoi = homNay.Year - ngay.Year;
        if (ngay > homNay.AddYears(-tuoi))
        {
            tuoi--;
        }

        if (ngay > homNay || tuoi is < TuoiNhoNhat or > TuoiLonNhat)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.ngay_sinh), $"Tuổi phải trong khoảng {TuoiNhoNhat}–{TuoiLonNhat}.");
        }
    }

    /// <summary>Giới tính tùy chọn: không gửi thì null; có gửi mà sai (kể cả chuỗi rỗng) là lỗi.</summary>
    private static GioiTinh? DocGioiTinh(HoSoLuuDto dto) =>
        dto.gioi_tinh?.Trim() switch
        {
            null => null,
            nameof(GioiTinh.nam) => GioiTinh.nam,
            nameof(GioiTinh.nu) => GioiTinh.nu,
            nameof(GioiTinh.khac) => GioiTinh.khac,
            nameof(GioiTinh.khong_muon_noi) => GioiTinh.khong_muon_noi,
            _ => throw new ValidationFailedException(nameof(HoSoLuuDto.gioi_tinh), "Giới tính không hợp lệ."),
        };

    private async Task KiemTraDanhMucAsync(
        HoSoLuuDto dto, LoaiNguoiDung loaiNguoiDung, DateOnly homNay, CancellationToken cancellationToken)
    {
        KiemTraNgaySinh(dto.ngay_sinh, homNay);
        await KiemTraMaTuyChonAsync(
            dto.khu_vuc_hoat_dong, KhuVuc, nameof(HoSoLuuDto.khu_vuc_hoat_dong),
            "khu vực", "Vui lòng chọn khu vực.", batBuoc: false, cancellationToken);

        var laHocVien = loaiNguoiDung is LoaiNguoiDung.hoc_vien or LoaiNguoiDung.ca_hai;
        var laPt = loaiNguoiDung is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai;
        await KiemTraKhoiHocVienAsync(dto, laHocVien, cancellationToken);
        await KiemTraKhoiPtAsync(dto, laPt, cancellationToken);
    }

    /// <summary>
    /// Khối học viên. <paramref name="batBuoc"/> (hoc_vien, ca_hai): đủ mục tiêu, trình độ, chiều cao, cân nặng. PT bỏ
    /// khối này được; nếu có gửi thì vẫn kiểm tra, và có một trong hai số đo thì phải có cả hai.
    /// </summary>
    private async Task KiemTraKhoiHocVienAsync(HoSoLuuDto dto, bool batBuoc, CancellationToken cancellationToken)
    {
        await KiemTraMaTuyChonAsync(
            dto.muc_tieu_tap_luyen, MucTieuTapLuyen, nameof(HoSoLuuDto.muc_tieu_tap_luyen),
            "mục tiêu tập luyện", "Vui lòng chọn mục tiêu tập luyện.", batBuoc, cancellationToken);
        await KiemTraMaTuyChonAsync(
            dto.trinh_do_tap, TrinhDoTap, nameof(HoSoLuuDto.trinh_do_tap),
            "trình độ tập", "Vui lòng chọn trình độ tập.", batBuoc, cancellationToken);

        var coSoDo = batBuoc || dto.chieu_cao_cm is not null || dto.can_nang_kg is not null;
        if (!coSoDo)
        {
            return;
        }

        if (dto.chieu_cao_cm is not { } chieuCao)
        {
            throw new ValidationFailedException(nameof(HoSoLuuDto.chieu_cao_cm), "Vui lòng nhập chiều cao.");
        }

        if (chieuCao is < ChieuCaoNhoNhat or > ChieuCaoLonNhat)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.chieu_cao_cm), $"Chiều cao phải trong khoảng {ChieuCaoNhoNhat}–{ChieuCaoLonNhat} cm.");
        }

        if (dto.can_nang_kg is not { } canNang)
        {
            throw new ValidationFailedException(nameof(HoSoLuuDto.can_nang_kg), "Vui lòng nhập cân nặng.");
        }

        if (canNang is < CanNangNhoNhat or > CanNangLonNhat)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.can_nang_kg), $"Cân nặng phải trong khoảng {CanNangNhoNhat}–{CanNangLonNhat} kg.");
        }
    }

    /// <summary>
    /// Khối PT. <paramref name="laPt"/> (pt, ca_hai): ít nhất 1 chuyên môn và có số năm kinh nghiệm. Học viên không được
    /// gửi khối này (<c>ROLE_INVALID</c>).
    /// </summary>
    private async Task KiemTraKhoiPtAsync(HoSoLuuDto dto, bool laPt, CancellationToken cancellationToken)
    {
        var coThongTinPt = dto.chuyen_mon is { Count: > 0 } || dto.so_nam_kinh_nghiem is not null
            || !string.IsNullOrWhiteSpace(dto.gioi_thieu);
        if (!laPt)
        {
            if (coThongTinPt)
            {
                throw new ValidationFailedException(
                    nameof(HoSoLuuDto.chuyen_mon),
                    "Chỉ tài khoản PT mới gửi thông tin PT.",
                    BizErrorCodes.RoleInvalid);
            }

            return;
        }

        var chuyenMon = ChuanHoaChuyenMon(dto.chuyen_mon) ?? [];
        if (chuyenMon.Count == 0)
        {
            throw new ValidationFailedException(nameof(HoSoLuuDto.chuyen_mon), "Vui lòng chọn ít nhất 1 chuyên môn.");
        }

        if (chuyenMon.Count > ChuyenMonToiDa)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.chuyen_mon), $"Chọn tối đa {ChuyenMonToiDa} chuyên môn.");
        }

        foreach (var ma in chuyenMon)
        {
            await KiemTraMaAsync(ma, ChuyenMonPt, nameof(HoSoLuuDto.chuyen_mon), "chuyên môn PT", cancellationToken);
        }

        if (dto.so_nam_kinh_nghiem is not { } soNam)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.so_nam_kinh_nghiem), "Vui lòng nhập số năm kinh nghiệm.");
        }

        if (soNam is < 0 or > KinhNghiemLonNhat)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.so_nam_kinh_nghiem), $"Số năm kinh nghiệm phải trong khoảng 0–{KinhNghiemLonNhat}.");
        }

        if (dto.gioi_thieu is { } gioiThieu && gioiThieu.Trim().Length > GioiThieuToiDa)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.gioi_thieu), $"Giới thiệu tối đa {GioiThieuToiDa} ký tự.");
        }
    }

    /// <summary>Mã danh mục của khối không bắt buộc: bỏ trống thì qua (hoặc báo thiếu khi bắt buộc), có giá trị thì phải đúng.</summary>
    private async Task KiemTraMaTuyChonAsync(
        string? ma, string maDinhDanh, string field, string tenLoai, string thongBaoThieu, bool batBuoc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ma))
        {
            if (batBuoc)
            {
                throw new ValidationFailedException(field, thongBaoThieu);
            }

            return;
        }

        await KiemTraMaAsync(ma, maDinhDanh, field, tenLoai, cancellationToken);
    }

    private async Task KiemTraMaAsync(
        string ma, string maDinhDanh, string field, string tenLoai, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ma)
            || !await danhMucTraCuu.BLKiemTraDangDungAsync(maDinhDanh, ma.Trim(), cancellationToken))
        {
            throw new ValidationFailedException(field, $"Mã {tenLoai} không hợp lệ.", BizErrorCodes.CategoryInvalid);
        }
    }

    private static string? ChuanHoaChuoi(string? giaTri) =>
        string.IsNullOrWhiteSpace(giaTri) ? null : giaTri.Trim();

    private static List<string>? ChuanHoaChuyenMon(List<string>? chuyenMon) =>
        chuyenMon is null
            ? null
            : chuyenMon.Select(ma => ma.Trim()).Where(ma => ma.Length > 0).Distinct(StringComparer.Ordinal).ToList();

    private static HoSoDto ToDto(ho_so_nguoi_dung hoSo, chi_so_co_the? chiSo) =>
        new(
            hoSo.loai_nguoi_dung.ToString(),
            hoSo.ten_hien_thi,
            hoSo.ngay_sinh,
            hoSo.gioi_tinh?.ToString(),
            hoSo.trinh_do_tap,
            hoSo.muc_tieu_tap_luyen,
            hoSo.khu_vuc_hoat_dong,
            hoSo.chuyen_mon,
            hoSo.so_nam_kinh_nghiem,
            hoSo.gioi_thieu,
            hoSo.trang_thai_xac_minh.ToString(),
            chiSo is null ? null : new ChiSoCoTheDto(chiSo.ngay_do, chiSo.chieu_cao_cm, chiSo.can_nang_kg));
}
