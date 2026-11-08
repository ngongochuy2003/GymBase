using GymBase.BLAuth.NguoiDung;

namespace GymBase.UnitTests.Auth;

/// <summary>Quy tắc dữ liệu nhập của người dùng và thông báo tiếng Việt.</summary>
public class NguoiDungValidatorTests
{
    private static readonly NguoiDungCreateValidator Validator = new();

    private static NguoiDungCreateDto Valid(
        string taiKhoan = "hv.moi", string hoTen = "Học viên", string? email = "a@gymbase.local", string? phone = "0912 345 678", string vaiTro = "Trainee") => new()
    {
        tai_khoan = taiKhoan,
        mat_khau = AuthTestHelpers.ValidPassword,
        ho_ten = hoTen,
        email = email,
        so_dien_thoai = phone,
        vai_tro = vaiTro,
    };

    private static string[] ErrorFields(NguoiDungCreateDto dto) =>
        Validator.Validate(dto).Errors.Select(e => e.PropertyName).ToArray();

    [Fact]
    public void Accepts_valid_body() => Assert.Empty(ErrorFields(Valid()));

    [Fact]
    public void Optional_fields_may_be_null() => Assert.Empty(ErrorFields(Valid(email: null, phone: null)));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("  ab  ")] // đủ 3 ký tự chỉ nhờ khoảng trắng
    public void Rejects_short_username(string value) => Assert.Equal(["tai_khoan"], ErrorFields(Valid(taiKhoan: value)));

    [Fact]
    public void Rejects_too_long_username() => Assert.Equal(["tai_khoan"], ErrorFields(Valid(taiKhoan: new string('a', 51))));

    [Theory]
    [InlineData("khong-co-a-cong")]
    [InlineData("")]
    public void Rejects_bad_email(string value) => Assert.Equal(["email"], ErrorFields(Valid(email: value)));

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public void Rejects_bad_phone(string value) => Assert.Equal(["so_dien_thoai"], ErrorFields(Valid(phone: value)));

    [Fact]
    public void Each_field_reports_only_its_first_error()
    {
        var errors = Validator.Validate(Valid(taiKhoan: "")).Errors;

        Assert.Single(errors);
    }

    [Fact]
    public void Messages_are_vietnamese_and_use_display_names()
    {
        var message = Validator.Validate(Valid(hoTen: "")).Errors.Single().ErrorMessage;

        Assert.Contains("Họ tên", message);
        Assert.DoesNotContain("must not be empty", message);
    }
}
