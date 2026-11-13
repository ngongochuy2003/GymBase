using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.BLAuth;
using GymBase.TestDoubles;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.MySql.DLTaiKhoan;
using GymBase.Entities.Auth;

namespace GymBase.UnitTests.Auth;

/// <summary>
/// Kiểm tra TaiKhoanService (CRUD lấy từ BaseService) trên dữ liệu mock của TaiKhoanRepository: 5 người dùng còn hiệu lực, 1 đã xóa.
/// Mỗi test có store riêng nên thêm/sửa/xóa không ảnh hưởng test khác.
/// </summary>
public class TaiKhoanServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("0193f0a0-0000-7000-8000-000000000001");
    private static readonly DateTime Now = new(2026, 11, 20, 8, 30, 0, DateTimeKind.Utc);

    private readonly InMemoryStore<tai_khoan> _store = new();

    private readonly TaiKhoanService _service;

    public TaiKhoanServiceTests()
    {
        _service = CreateService(new FakeCurrentUser("nv.thu"));
    }

    private TaiKhoanService CreateService(FakeCurrentUser currentUser) => new(
        new InMemoryTaiKhoanRepository(_store),
        new FixedTimeProvider(Now),
        currentUser,
        new TaiKhoanCreateValidator(),
        new TaiKhoanUpdateValidator(),
        AuthTestHelpers.FastHasher);

    [Fact]
    public async Task Invalid_body_is_rejected_before_touching_data()
    {
        var before = _store.All().Count;

        var error = await Assert.ThrowsAsync<ValidationFailedException>(
            () => _service.BLCreateAsync(new TaiKhoanCreateDto { ten_dang_nhap = null!, ho_ten = "", email = "a@gymbase.local", vai_tro = "User" }, CancellationToken.None));

        Assert.Equal(["ho_ten", "mat_khau", "ten_dang_nhap"], error.Errors!.Keys.Order());
        Assert.Equal(before, _store.All().Count);
    }

    [Fact]
    public async Task Update_validates_body_before_checking_the_record_exists()
    {
        await Assert.ThrowsAsync<ValidationFailedException>(
            () => _service.BLUpdateAsync(Guid.NewGuid(), new TaiKhoanUpdateDto(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_and_update_record_the_current_user()
    {
        var created = await _service.BLCreateAsync(NewUser, CancellationToken.None);
        Assert.Equal("nv.thu", _store.Find(created.Id)!.nguoi_tao);

        var update = new TaiKhoanUpdateDto { ho_ten = "Tên mới", email = "admin@gymbase.local", vai_tro = "User" };
        await _service.BLUpdateAsync(AdminId, update, CancellationToken.None);
        Assert.Equal("nv.thu", _store.Find(AdminId)!.nguoi_chinh_sua);
    }

    [Fact]
    public async Task Audit_user_is_null_when_not_signed_in()
    {
        var service = CreateService(new FakeCurrentUser());

        var created = await service.BLCreateAsync(NewUser, CancellationToken.None);

        Assert.Null(_store.Find(created.Id)!.nguoi_tao);
    }

    private static readonly TaiKhoanCreateDto NewUser = NewUserWith(" hv.moi ");

    private static TaiKhoanCreateDto NewUserWith(string tenDangNhap) => new()
    {
        ten_dang_nhap = tenDangNhap,
        mat_khau = AuthTestHelpers.ValidPassword,
        ho_ten = "Học viên mới",
        email = "moi@gymbase.local",
        vai_tro = "User",
    };

    [Fact]
    public async Task Paging_sorts_newest_first()
    {
        var result = await _service.BLGetPagingAsync(new PagingRequest(), CancellationToken.None);

        Assert.Equal(5, result.meta.total);
        Assert.Equal("hv.phuong", result.data[0].ten_dang_nhap);
        Assert.Equal("admin", result.data[^1].ten_dang_nhap);
    }

    [Fact]
    public async Task Paging_splits_pages()
    {
        var result = await _service.BLGetPagingAsync(new PagingRequest { page = 2, page_size = 2 }, CancellationToken.None);

        Assert.Equal(2, result.data.Count);
        Assert.Equal(5, result.meta.total);
        Assert.Equal(3, result.meta.total_page);
        Assert.Equal(new PagingRange(3, 4), result.meta.ranger);
        Assert.Equal("pt.lan", result.data[0].ten_dang_nhap);
    }

    [Fact]
    public async Task Search_matches_name_case_insensitively()
    {
        var result = await _service.BLGetPagingAsync(new PagingRequest { search = "  PT. " }, CancellationToken.None);

        Assert.Equal(2, result.meta.total);
        Assert.All(result.data, u => Assert.Equal("User", u.vai_tro));
    }

    [Theory]
    [InlineData(0, 0, 1, PagingRequest.DefaultPageSize)]
    [InlineData(-3, 500, 1, PagingRequest.MaxPageSize)]
    public async Task Invalid_paging_values_are_normalized(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var result = await _service.BLGetPagingAsync(new PagingRequest { page = page, page_size = pageSize }, CancellationToken.None);

        Assert.Equal(expectedPage, result.meta.page);
        Assert.Equal(expectedPageSize, result.meta.page_size);
    }

    [Fact]
    public async Task GetById_returns_user()
    {
        var user = await _service.BLGetByIdAsync(AdminId, CancellationToken.None);

        Assert.Equal("admin", user.ten_dang_nhap);
    }

    [Fact]
    public async Task GetById_throws_not_found_for_missing_user()
    {
        var missing = await Assert.ThrowsAsync<NotFoundException>(() => _service.BLGetByIdAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(ErrorCodes.NotFound, missing.Code);
        Assert.Equal("Không tìm thấy người dùng hoặc người dùng đã bị xóa.", missing.Message);
    }

    [Fact]
    public async Task Create_assigns_uuidv7_and_utc_audit_then_is_readable()
    {
        var created = await _service.BLCreateAsync(NewUser, CancellationToken.None);

        Assert.Equal(7, created.Id.Version);
        Assert.Equal("hv.moi", created.ten_dang_nhap);
        Assert.Equal(Now, created.ngay_tao);
        Assert.Null(created.ngay_chinh_sua);
        Assert.True(created.dang_hoat_dong);

        var page = await _service.BLGetPagingAsync(new PagingRequest(), CancellationToken.None);
        Assert.Equal(6, page.meta.total);
        Assert.Equal(created.Id, page.data[0].Id);
    }

    [Theory]
    [InlineData("pt.minh")]
    [InlineData("  PT.Minh ")] // không phân biệt hoa thường, bỏ khoảng trắng
    public async Task Create_rejects_username_of_active_user(string tenDangNhap)
    {
        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _service.BLCreateAsync(NewUserWith(tenDangNhap), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.UserUsernameTaken, ex.Code);
        Assert.Equal(BusinessErrorKind.Conflict, ex.Kind);
        Assert.Equal(["ten_dang_nhap"], ex.Errors!.Keys);
        Assert.Equal(5, (await _service.BLGetPagingAsync(new PagingRequest(), CancellationToken.None)).meta.total);
    }

    [Fact]
    public async Task Update_changes_allowed_fields_and_sets_modified_time()
    {
        var update = new TaiKhoanUpdateDto { ho_ten = "Admin mới", email = "admin@gymbase.local", vai_tro = "Admin", dang_hoat_dong = false };

        var updated = await _service.BLUpdateAsync(AdminId, update, CancellationToken.None);

        Assert.Equal("Admin mới", updated.ho_ten);
        Assert.Equal("admin", updated.ten_dang_nhap);
        Assert.False(updated.dang_hoat_dong);
        Assert.Equal(Now, updated.ngay_chinh_sua);
        Assert.Equal("Admin mới", (await _service.BLGetByIdAsync(AdminId, CancellationToken.None)).ho_ten);
    }

    [Fact]
    public async Task Update_throws_not_found_for_missing_user()
    {
        var update = new TaiKhoanUpdateDto { ho_ten = "X", email = "x@gymbase.local", vai_tro = "User" };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.BLUpdateAsync(Guid.NewGuid(), update, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_removes_user_once()
    {
        await _service.BLDeleteAsync(AdminId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.BLGetByIdAsync(AdminId, CancellationToken.None));
        Assert.Equal(4, (await _service.BLGetPagingAsync(new PagingRequest(), CancellationToken.None)).meta.total);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.BLDeleteAsync(AdminId, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.BLDeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
