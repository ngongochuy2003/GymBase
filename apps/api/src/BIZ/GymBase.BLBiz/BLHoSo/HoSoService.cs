using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.DanhMuc;
using GymBase.ModuleContracts.TepTin;

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
    TimeProvider timeProvider,
    IValidator<HoSoLuuDto> validator,
    IChungChiPtLienKet chungChiLienKet,
    IAnhDaiDienLienKet anhDaiDienLienKet)
{
    private const string MucTieuTapLuyen = "muc_tieu_tap_luyen";
    private const string KhuVuc = "khu_vuc";
    private const string ChuyenMonPt = "chuyen_mon_pt";

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
        var chungChi = await chungChiLienKet.BLLayChungChiAsync(taiKhoanId, cancellationToken);
        return ToDto(hoSo, chiSo, chungChi);
    }

    /// <summary>
    /// Lưu hồ sơ và chỉ số cơ thể trong một transaction. Chỉ thêm dòng <c>chi_so_co_the</c> khi chiều cao hoặc cân nặng
    /// khác bản ghi mới nhất. Không gửi <c>ten_hien_thi</c>
    /// hoặc ảnh đại diện thì giữ giá trị đang có; không đổi trạng thái xác minh.
    /// </summary>
    public async Task<HoSoDto> BLLuuHoSoAsync(HoSoLuuDto dto, CancellationToken cancellationToken)
    {
        await KiemTraDuLieuAsync(dto, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var loaiNguoiDung = HoSoLuuValidator.DocLoaiNguoiDung(dto.loai_nguoi_dung)!.Value;
        var gioiTinh = HoSoLuuValidator.DocGioiTinh(dto.gioi_tinh);
        var tenHienThi = dto.ten_hien_thi?.Trim();
        await KiemTraDanhMucAsync(dto, loaiNguoiDung, cancellationToken);

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
        hoSo.chuyen_mon = dto.chuyen_mon is null ? null : HoSoLuuValidator.ChuanHoaChuyenMon(dto.chuyen_mon);
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

        // Chứng chỉ chỉ có ở khối PT: gửi file_id thì gắn tệp, không gửi hoặc đổi sang học viên thì gỡ chứng chỉ đang có.
        var laPt = loaiNguoiDung is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai;
        var chungChi = await chungChiLienKet.BLDatChungChiAsync(
            taiKhoanId, laPt ? dto.chung_chi_tep_tin_id : null, cancellationToken);

        if (dto.anh_dai_dien_tep_tin_id is { } anhMoiId)
        {
            await anhDaiDienLienKet.BLGanAnhDaiDienAsync(taiKhoanId, anhMoiId, cancellationToken);
            hoSo.anh_dai_dien_tep_tin_id = anhMoiId;
        }

        await hoSoRepository.DLUpsertAsync(hoSo, chiSoMoi, cancellationToken);

        // Xóa ảnh cũ sau khi hồ sơ đã trỏ sang ảnh mới, để lưu lỗi giữa chừng không làm mất ảnh đang hiện.
        if (dto.anh_dai_dien_tep_tin_id is { } anhGiuLaiId)
        {
            await anhDaiDienLienKet.BLGoAnhCuAsync(taiKhoanId, anhGiuLaiId, cancellationToken);
        }

        return ToDto(hoSo, chiSoMoi ?? moiNhat, chungChi);
    }

    private Guid TaiKhoanIdHienTai() => currentUser.UserId ?? throw new UnauthorizedException();

    /// <summary>
    /// Chạy <see cref="HoSoLuuValidator"/>. Lỗi dùng sai vai trò mang mã <see cref="BizErrorCodes.RoleInvalid"/>, các lỗi
    /// khác là <c>VALIDATION_FAILED</c>.
    /// </summary>
    private async Task KiemTraDuLieuAsync(HoSoLuuDto dto, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(dto, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        throw result.Errors.Any(error => error.ErrorCode == BizErrorCodes.RoleInvalid)
            ? new ValidationFailedException(errors, code: BizErrorCodes.RoleInvalid)
            : new ValidationFailedException(errors);
    }

    /// <summary>Mã danh mục phải tồn tại và đang dùng; dữ liệu đã qua <see cref="HoSoLuuValidator"/> nên chỉ còn tra DB.</summary>
    private async Task KiemTraDanhMucAsync(HoSoLuuDto dto, LoaiNguoiDung loaiNguoiDung, CancellationToken cancellationToken)
    {
        await KiemTraMaTuyChonAsync(dto.khu_vuc_hoat_dong, KhuVuc, nameof(HoSoLuuDto.khu_vuc_hoat_dong), "khu vực", cancellationToken);
        await KiemTraMaTuyChonAsync(
            dto.muc_tieu_tap_luyen, MucTieuTapLuyen, nameof(HoSoLuuDto.muc_tieu_tap_luyen), "mục tiêu tập luyện", cancellationToken);
        await KiemTraMaTuyChonAsync(dto.trinh_do_tap, TrinhDoTap, nameof(HoSoLuuDto.trinh_do_tap), "trình độ tập", cancellationToken);

        if (loaiNguoiDung is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai)
        {
            foreach (var ma in HoSoLuuValidator.ChuanHoaChuyenMon(dto.chuyen_mon))
            {
                await KiemTraMaAsync(ma, ChuyenMonPt, nameof(HoSoLuuDto.chuyen_mon), "chuyên môn PT", cancellationToken);
            }
        }
    }

    /// <summary>Bỏ trống thì qua (khối bắt buộc đã được validator kiểm tra), có giá trị thì phải là mã đang dùng.</summary>
    private async Task KiemTraMaTuyChonAsync(
        string? ma, string maDinhDanh, string field, string tenLoai, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(ma))
        {
            await KiemTraMaAsync(ma, maDinhDanh, field, tenLoai, cancellationToken);
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

    private static string? ChuanHoaChuoi(string? giaTri) =>
        string.IsNullOrWhiteSpace(giaTri) ? null : giaTri.Trim();

    private static HoSoDto ToDto(ho_so_nguoi_dung hoSo, chi_so_co_the? chiSo, ChungChiPtDto? chungChi) =>
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
            chiSo is null ? null : new ChiSoCoTheDto(chiSo.ngay_do, chiSo.chieu_cao_cm, chiSo.can_nang_kg),
            chungChi);
}
