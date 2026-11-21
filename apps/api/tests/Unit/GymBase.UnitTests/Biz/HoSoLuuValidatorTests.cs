using GymBase.BLBiz;
using GymBase.BLBiz.BLHoSo;

namespace GymBase.UnitTests.Biz;

/// <summary>Luật nhập của hồ sơ, không cần database: khối bắt buộc theo vai trò, khoảng giá trị và mã lỗi.</summary>
public class HoSoLuuValidatorTests
{
    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private static readonly HoSoLuuValidator Validator = new(
        new FixedTimeProvider(new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc)));

    private static HoSoLuuDto HocVien(decimal chieuCao = 172.5m) => new()
    {
        loai_nguoi_dung = "hoc_vien",
        muc_tieu_tap_luyen = "tang_co",
        trinh_do_tap = "co_nen_tang",
        chieu_cao_cm = chieuCao,
        can_nang_kg = 68.4m,
    };

    private static HoSoLuuDto Pt(DateOnly? ngaySinh = null, string? gioiTinh = null, string? tenHienThi = null) => new()
    {
        loai_nguoi_dung = "pt",
        chuyen_mon = ["yoga"],
        so_nam_kinh_nghiem = 3,
        ngay_sinh = ngaySinh,
        gioi_tinh = gioiTinh,
        ten_hien_thi = tenHienThi,
    };

    private static string[] Fields(HoSoLuuDto dto) =>
        Validator.Validate(dto).Errors.Select(e => e.PropertyName).Order().ToArray();

    [Fact]
    public void Accepts_a_complete_student_profile() => Assert.Empty(Fields(HocVien()));

    [Fact]
    public void Missing_role_is_a_plain_validation_error()
    {
        var error = Validator.Validate(new HoSoLuuDto()).Errors.Single();

        Assert.Equal("loai_nguoi_dung", error.PropertyName);
        Assert.NotEqual(BizErrorCodes.RoleInvalid, error.ErrorCode);
    }

    [Fact]
    public void Unknown_role_carries_the_role_invalid_code()
    {
        var error = Validator.Validate(new HoSoLuuDto { loai_nguoi_dung = "admin" }).Errors.Single();

        Assert.Equal(BizErrorCodes.RoleInvalid, error.ErrorCode);
    }

    [Fact]
    public void Student_sending_pt_data_carries_the_role_invalid_code()
    {
        var dto = new HoSoLuuDto
        {
            loai_nguoi_dung = "hoc_vien",
            muc_tieu_tap_luyen = "tang_co",
            trinh_do_tap = "co_nen_tang",
            chieu_cao_cm = 170,
            can_nang_kg = 60,
            chuyen_mon = ["yoga"],
        };

        var error = Validator.Validate(dto).Errors.Single();

        Assert.Equal("chuyen_mon", error.PropertyName);
        Assert.Equal(BizErrorCodes.RoleInvalid, error.ErrorCode);
    }

    [Fact]
    public void Pt_may_skip_the_student_block_but_needs_the_pt_block()
    {
        Assert.Empty(Fields(Pt()));
        Assert.Equal(["chuyen_mon", "so_nam_kinh_nghiem"], Fields(new HoSoLuuDto { loai_nguoi_dung = "pt" }));
    }

    [Fact]
    public void Both_roles_need_both_blocks()
    {
        var fields = Fields(new HoSoLuuDto { loai_nguoi_dung = "ca_hai" });

        Assert.Equal(
            new[] { "can_nang_kg", "chieu_cao_cm", "chuyen_mon", "muc_tieu_tap_luyen", "so_nam_kinh_nghiem", "trinh_do_tap" },
            fields);
    }

    [Fact]
    public void Duplicate_specialties_count_once()
    {
        var dto = new HoSoLuuDto
        {
            loai_nguoi_dung = "pt",
            so_nam_kinh_nghiem = 1,
            chuyen_mon = [.. Enumerable.Repeat("yoga", 20)],
        };

        Assert.Empty(Fields(dto));
    }

    [Theory]
    [InlineData("2026-01-01")] // chưa đủ 13 tuổi
    [InlineData("1900-01-01")] // quá 100 tuổi
    [InlineData("2030-01-01")] // ở tương lai
    public void Rejects_birth_dates_outside_the_age_range(string date) =>
        Assert.Equal(["ngay_sinh"], Fields(Pt(ngaySinh: DateOnly.Parse(date))));

    [Theory]
    [InlineData(99)]
    [InlineData(251)]
    public void Rejects_height_outside_the_range(int height) =>
        Assert.Equal(["chieu_cao_cm"], Fields(HocVien(height)));

    [Theory]
    [InlineData("")]
    [InlineData("tuy_chon")]
    public void Rejects_unknown_or_empty_gender(string gender) =>
        Assert.Equal(["gioi_tinh"], Fields(Pt(gioiTinh: gender)));

    [Fact]
    public void Rejects_an_empty_display_name() =>
        Assert.Equal(["ten_hien_thi"], Fields(Pt(tenHienThi: "  ")));
}
