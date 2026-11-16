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
        var loaiNguoiDung = DocLoaiNguoiDung(dto);
        await KiemTraDanhMucAsync(dto, loaiNguoiDung, cancellationToken);

        var taiKhoanId = TaiKhoanIdHienTai();
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // AsNoTracking: entity trả về được sửa rồi ghi lại trong một lần ở DL.
        var hoSo = await hoSoRepository.DLFindByTaiKhoanIdAsync(taiKhoanId, cancellationToken);
        if (hoSo is null)
        {
            hoSo = new ho_so_nguoi_dung
            {
                tai_khoan_id = taiKhoanId,
                ten_hien_thi = await hoTenProvider.BLDocHoTenAsync(taiKhoanId, cancellationToken)
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
        }

        hoSo.loai_nguoi_dung = loaiNguoiDung;
        hoSo.muc_tieu_tap_luyen = dto.muc_tieu_tap_luyen.Trim();
        hoSo.khu_vuc_hoat_dong = dto.khu_vuc_hoat_dong.Trim();
        hoSo.chuyen_mon = ChuanHoaChuyenMon(dto.chuyen_mon);
        hoSo.so_nam_kinh_nghiem = dto.so_nam_kinh_nghiem;
        hoSo.gioi_thieu = string.IsNullOrWhiteSpace(dto.gioi_thieu) ? null : dto.gioi_thieu.Trim();

        var chieuCao = Math.Round(dto.chieu_cao_cm!.Value, 1);
        var canNang = Math.Round(dto.can_nang_kg!.Value, 2);
        var moiNhat = await chiSoRepository.DLFindMoiNhatAsync(taiKhoanId, cancellationToken);

        // TODO(PROFILE-PT): khi dựng khối PT, tách chiều cao/cân nặng của học viên với hồ sơ PT nếu thiết kế đổi.
        chi_so_co_the? chiSoMoi = null;
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

    private async Task KiemTraDanhMucAsync(
        HoSoLuuDto dto, LoaiNguoiDung loaiNguoiDung, CancellationToken cancellationToken)
    {
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

        await KiemTraMaAsync(dto.muc_tieu_tap_luyen, MucTieuTapLuyen, nameof(HoSoLuuDto.muc_tieu_tap_luyen), "mục tiêu tập luyện", cancellationToken);
        await KiemTraMaAsync(dto.khu_vuc_hoat_dong, KhuVuc, nameof(HoSoLuuDto.khu_vuc_hoat_dong), "khu vực", cancellationToken);

        var laPt = loaiNguoiDung is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai;
        var coThongTinPt = dto.chuyen_mon is { Count: > 0 } || dto.so_nam_kinh_nghiem is not null
            || !string.IsNullOrWhiteSpace(dto.gioi_thieu);
        if (!laPt && coThongTinPt)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.chuyen_mon),
                "Chỉ tài khoản PT mới gửi thông tin PT.",
                BizErrorCodes.RoleInvalid);
        }

        if (dto.chuyen_mon is { } chuyenMon)
        {
            if (chuyenMon.Count > ChuyenMonToiDa)
            {
                throw new ValidationFailedException(
                    nameof(HoSoLuuDto.chuyen_mon), $"Chọn tối đa {ChuyenMonToiDa} chuyên môn.");
            }

            foreach (var ma in chuyenMon)
            {
                await KiemTraMaAsync(ma, ChuyenMonPt, nameof(HoSoLuuDto.chuyen_mon), "chuyên môn PT", cancellationToken);
            }
        }

        if (dto.so_nam_kinh_nghiem is < 0 or > KinhNghiemLonNhat)
        {
            throw new ValidationFailedException(
                nameof(HoSoLuuDto.so_nam_kinh_nghiem), $"Số năm kinh nghiệm phải trong khoảng 0–{KinhNghiemLonNhat}.");
        }
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

    private static List<string>? ChuanHoaChuyenMon(List<string>? chuyenMon) =>
        chuyenMon is null
            ? null
            : chuyenMon.Select(ma => ma.Trim()).Where(ma => ma.Length > 0).Distinct(StringComparer.Ordinal).ToList();

    private static HoSoDto ToDto(ho_so_nguoi_dung hoSo, chi_so_co_the? chiSo) =>
        new(
            hoSo.loai_nguoi_dung.ToString(),
            hoSo.ten_hien_thi,
            hoSo.muc_tieu_tap_luyen,
            hoSo.khu_vuc_hoat_dong,
            hoSo.chuyen_mon,
            hoSo.so_nam_kinh_nghiem,
            hoSo.gioi_thieu,
            hoSo.trang_thai_xac_minh.ToString(),
            chiSo is null ? null : new ChiSoCoTheDto(chiSo.ngay_do, chiSo.chieu_cao_cm, chiSo.can_nang_kg));
}
