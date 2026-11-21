using FluentValidation;
using GymBase.Base.Validation;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLBiz.BLHoSo;

/// <summary>
/// Quy tắc dữ liệu nhập của <see cref="HoSoLuuDto"/> không cần đọc database. Mã danh mục (cần tra
/// <c>IDanhMucTraCuu</c>) do <see cref="HoSoService"/> kiểm tra sau bước này. Khối nào bắt buộc tùy
/// <see cref="HoSoLuuDto.loai_nguoi_dung"/>. Lỗi dùng sai vai trò mang mã <see cref="BizErrorCodes.RoleInvalid"/>.
/// </summary>
public sealed class HoSoLuuValidator : AppValidator<HoSoLuuDto>
{
    public const int ChuyenMonToiDa = 10;
    public const decimal ChieuCaoNhoNhat = 100;
    public const decimal ChieuCaoLonNhat = 250;
    public const decimal CanNangNhoNhat = 20;
    public const decimal CanNangLonNhat = 300;
    public const short KinhNghiemLonNhat = 60;
    public const int GioiThieuToiDa = 1000;
    public const int TenHienThiToiDa = 100;
    public const int TuoiNhoNhat = 13;
    public const int TuoiLonNhat = 100;

    public HoSoLuuValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.loai_nguoi_dung).NotEmpty().WithMessage("Vui lòng chọn vai trò sử dụng.")
            .Must(value => DocLoaiNguoiDung(value) is not null)
            .WithMessage("Vai trò sử dụng không hợp lệ.").WithErrorCode(BizErrorCodes.RoleInvalid);

        RuleFor(x => x.ten_hien_thi!)
            .Must(ten => ten.Trim().Length is >= 1 and <= TenHienThiToiDa)
            .WithMessage($"Tên hiển thị phải từ 1 đến {TenHienThiToiDa} ký tự.")
            .When(x => x.ten_hien_thi is not null)
            .OverridePropertyName(nameof(HoSoLuuDto.ten_hien_thi));

        RuleFor(x => x.ngay_sinh).Must(ngay => TuoiHopLe(ngay!.Value, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)))
            .WithMessage($"Tuổi phải trong khoảng {TuoiNhoNhat}–{TuoiLonNhat}.")
            .When(x => x.ngay_sinh is not null);

        // Gửi chuỗi rỗng cũng là sai; không gửi (null) thì bỏ qua.
        RuleFor(x => x.gioi_tinh!)
            .Must(value => DocGioiTinh(value) is not null).WithMessage("Giới tính không hợp lệ.")
            .When(x => x.gioi_tinh is not null)
            .OverridePropertyName(nameof(HoSoLuuDto.gioi_tinh));

        When(x => DocLoaiNguoiDung(x.loai_nguoi_dung) is not null, () =>
        {
            KhoiHocVien();
            KhoiPt();
        });
    }

    /// <summary>
    /// Khối học viên. Bắt buộc với <c>hoc_vien</c> và <c>ca_hai</c>: mục tiêu, trình độ, chiều cao, cân nặng. PT bỏ khối này
    /// được; nếu có gửi số đo thì phải đủ cả chiều cao lẫn cân nặng và đúng khoảng.
    /// </summary>
    private void KhoiHocVien()
    {
        RuleFor(x => x.muc_tieu_tap_luyen).NotEmpty().WithMessage("Vui lòng chọn mục tiêu tập luyện.")
            .When(x => LaHocVien(x));
        RuleFor(x => x.trinh_do_tap).NotEmpty().WithMessage("Vui lòng chọn trình độ tập.")
            .When(x => LaHocVien(x));

        When(CoSoDo, () =>
        {
            RuleFor(x => x.chieu_cao_cm).NotNull().WithMessage("Vui lòng nhập chiều cao.")
                .Must(value => value >= ChieuCaoNhoNhat && value <= ChieuCaoLonNhat)
                .WithMessage($"Chiều cao phải trong khoảng {ChieuCaoNhoNhat}–{ChieuCaoLonNhat} cm.");
            RuleFor(x => x.can_nang_kg).NotNull().WithMessage("Vui lòng nhập cân nặng.")
                .Must(value => value >= CanNangNhoNhat && value <= CanNangLonNhat)
                .WithMessage($"Cân nặng phải trong khoảng {CanNangNhoNhat}–{CanNangLonNhat} kg.");
        });
    }

    /// <summary>
    /// Khối PT. Bắt buộc với <c>pt</c> và <c>ca_hai</c>: ít nhất 1 chuyên môn và số năm kinh nghiệm. Học viên không được gửi
    /// khối này (<see cref="BizErrorCodes.RoleInvalid"/>).
    /// </summary>
    private void KhoiPt()
    {
        RuleFor(x => x.chuyen_mon)
            .Must(_ => false).WithMessage("Chỉ tài khoản PT mới gửi thông tin PT.").WithErrorCode(BizErrorCodes.RoleInvalid)
            .When(x => !LaPt(x) && CoThongTinPt(x));

        When(LaPt, () =>
        {
            RuleFor(x => x.chuyen_mon).Must(value => ChuanHoaChuyenMon(value).Count > 0)
                .WithMessage("Vui lòng chọn ít nhất 1 chuyên môn.")
                .Must(value => ChuanHoaChuyenMon(value).Count <= ChuyenMonToiDa)
                .WithMessage($"Chọn tối đa {ChuyenMonToiDa} chuyên môn.");
            RuleFor(x => x.so_nam_kinh_nghiem).NotNull().WithMessage("Vui lòng nhập số năm kinh nghiệm.")
                .Must(value => value >= 0 && value <= KinhNghiemLonNhat)
                .WithMessage($"Số năm kinh nghiệm phải trong khoảng 0–{KinhNghiemLonNhat}.");
            RuleFor(x => x.gioi_thieu!).Must(value => value.Trim().Length <= GioiThieuToiDa)
                .WithMessage($"Giới thiệu tối đa {GioiThieuToiDa} ký tự.")
                .When(x => x.gioi_thieu is not null)
                .OverridePropertyName(nameof(HoSoLuuDto.gioi_thieu));
        });
    }

    /// <summary>Loại người dùng đã được kiểm tra ở bước trước nên luôn đọc được.</summary>
    public static LoaiNguoiDung? DocLoaiNguoiDung(string? value) =>
        value?.Trim() switch
        {
            nameof(LoaiNguoiDung.hoc_vien) => LoaiNguoiDung.hoc_vien,
            nameof(LoaiNguoiDung.pt) => LoaiNguoiDung.pt,
            nameof(LoaiNguoiDung.ca_hai) => LoaiNguoiDung.ca_hai,
            _ => null,
        };

    public static GioiTinh? DocGioiTinh(string? value) =>
        value?.Trim() switch
        {
            nameof(GioiTinh.nam) => GioiTinh.nam,
            nameof(GioiTinh.nu) => GioiTinh.nu,
            nameof(GioiTinh.khac) => GioiTinh.khac,
            nameof(GioiTinh.khong_muon_noi) => GioiTinh.khong_muon_noi,
            _ => null,
        };

    public static bool LaHocVien(HoSoLuuDto dto) => DocLoaiNguoiDung(dto.loai_nguoi_dung) is LoaiNguoiDung.hoc_vien or LoaiNguoiDung.ca_hai;

    public static bool LaPt(HoSoLuuDto dto) => DocLoaiNguoiDung(dto.loai_nguoi_dung) is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai;

    /// <summary>Cắt khoảng trắng, bỏ mục rỗng và mục trùng; giữ thứ tự.</summary>
    public static List<string> ChuanHoaChuyenMon(IEnumerable<string>? chuyenMon) =>
        chuyenMon is null
            ? []
            : chuyenMon.Select(ma => ma.Trim()).Where(ma => ma.Length > 0).Distinct(StringComparer.Ordinal).ToList();

    private static bool CoSoDo(HoSoLuuDto dto) => LaHocVien(dto) || dto.chieu_cao_cm is not null || dto.can_nang_kg is not null;

    private static bool CoThongTinPt(HoSoLuuDto dto) =>
        dto.chuyen_mon is { Count: > 0 } || dto.so_nam_kinh_nghiem is not null || !string.IsNullOrWhiteSpace(dto.gioi_thieu);

    private static bool TuoiHopLe(DateOnly ngaySinh, DateOnly homNay)
    {
        var tuoi = homNay.Year - ngaySinh.Year;
        if (ngaySinh > homNay.AddYears(-tuoi))
        {
            tuoi--;
        }

        return ngaySinh <= homNay && tuoi is >= TuoiNhoNhat and <= TuoiLonNhat;
    }
}
