using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.BLAuth;
using GymBase.TestDoubles;
using GymBase.BLAuth.NguoiDung;
using GymBase.DLAuth.MySql;
using GymBase.DLAuth.Contract.Entities;

namespace GymBase.UnitTests.Auth;

/// <summary>
/// Kiểm tra UserService (CRUD lấy từ BaseService) trên dữ liệu mock của UserRepository: 5 người dùng còn hiệu lực, 1 đã xóa.
/// Mỗi test có store riêng nên thêm/sửa/xóa không ảnh hưởng test khác.
/// </summary>
public class UserServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("0193f0a0-0000-7000-8000-000000000001");
    private static readonly Guid DeletedId = Guid.Parse("0193f0a0-0000-7000-8000-000000000006");
    private static readonly DateTime Now = new(2026, 11, 20, 8, 30, 0, DateTimeKind.Utc);

    private readonly InMemoryStore<nguoi_dung> _store = new();

    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(new InMemoryUserRepository(_store), new FixedTimeProvider(Now), new FakeCurrentUser("nv.thu"));
    }

    [Fact]
    public async Task Create_and_update_record_the_current_user()
    {
        var created = await _service.CreateAsync(NewUser, CancellationToken.None);
        Assert.Equal("nv.thu", _store.Find(created.Id)!.nguoi_tao);

        var update = new NguoiDungUpdateDto { ho_ten = "Tên mới", vai_tro = "Trainee" };
        await _service.UpdateAsync(AdminId, update, CancellationToken.None);
        Assert.Equal("nv.thu", _store.Find(AdminId)!.nguoi_chinh_sua);
    }

    [Fact]
    public async Task Audit_user_is_null_when_not_signed_in()
    {
        var service = new UserService(new InMemoryUserRepository(_store), new FixedTimeProvider(Now), new FakeCurrentUser());

        var created = await service.CreateAsync(NewUser, CancellationToken.None);

        Assert.Null(_store.Find(created.Id)!.nguoi_tao);
    }

    private static readonly NguoiDungCreateDto NewUser = NewUserWith(" hv.moi ");

    private static NguoiDungCreateDto NewUserWith(string taiKhoan) => new()
    {
        tai_khoan = taiKhoan,
        ho_ten = "Học viên mới",
        email = "moi@gymbase.local",
        vai_tro = "Trainee",
    };

    [Fact]
    public async Task Paging_hides_deleted_users_and_sorts_newest_first()
    {
        var result = await _service.GetPagingAsync(new PagingRequest(), CancellationToken.None);

        Assert.Equal(5, result.meta.total);
        Assert.DoesNotContain(result.data, u => u.Id == DeletedId);
        Assert.Equal("hv.phuong", result.data[0].tai_khoan);
        Assert.Equal("admin", result.data[^1].tai_khoan);
    }

    [Fact]
    public async Task Paging_splits_pages()
    {
        var result = await _service.GetPagingAsync(new PagingRequest { page = 2, page_size = 2 }, CancellationToken.None);

        Assert.Equal(2, result.data.Count);
        Assert.Equal(5, result.meta.total);
        Assert.Equal(3, result.meta.total_page);
        Assert.Equal(new PagingRange(3, 4), result.meta.ranger);
        Assert.Equal("pt.lan", result.data[0].tai_khoan);
    }

    [Fact]
    public async Task Search_matches_name_case_insensitively()
    {
        var result = await _service.GetPagingAsync(new PagingRequest { search = "  PT. " }, CancellationToken.None);

        Assert.Equal(2, result.meta.total);
        Assert.All(result.data, u => Assert.Equal("Trainer", u.vai_tro));
    }

    [Theory]
    [InlineData(0, 0, 1, PagingRequest.DefaultPageSize)]
    [InlineData(-3, 500, 1, PagingRequest.MaxPageSize)]
    public async Task Invalid_paging_values_are_normalized(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var result = await _service.GetPagingAsync(new PagingRequest { page = page, page_size = pageSize }, CancellationToken.None);

        Assert.Equal(expectedPage, result.meta.page);
        Assert.Equal(expectedPageSize, result.meta.page_size);
    }

    [Fact]
    public async Task GetById_returns_user()
    {
        var user = await _service.GetByIdAsync(AdminId, CancellationToken.None);

        Assert.Equal("admin", user.tai_khoan);
    }

    [Fact]
    public async Task GetById_throws_not_found_for_missing_or_deleted_user()
    {
        var missing = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(DeletedId, CancellationToken.None));

        Assert.Equal(ErrorCodes.NotFound, missing.Code);
        Assert.Equal("Không tìm thấy người dùng hoặc người dùng đã bị xóa.", missing.Message);
    }

    [Fact]
    public async Task Create_assigns_uuidv7_and_utc_audit_then_is_readable()
    {
        var created = await _service.CreateAsync(NewUser, CancellationToken.None);

        Assert.Equal(7, created.Id.Version);
        Assert.Equal("hv.moi", created.tai_khoan);
        Assert.Equal(Now, created.ngay_tao);
        Assert.Null(created.ngay_chinh_sua);
        Assert.True(created.dang_hoat_dong);

        var page = await _service.GetPagingAsync(new PagingRequest(), CancellationToken.None);
        Assert.Equal(6, page.meta.total);
        Assert.Equal(created.Id, page.data[0].Id);
    }

    [Theory]
    [InlineData("pt.minh")]
    [InlineData("  PT.Minh ")] // không phân biệt hoa thường, bỏ khoảng trắng
    public async Task Create_rejects_username_of_active_user(string taiKhoan)
    {
        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(NewUserWith(taiKhoan), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.UserUsernameTaken, ex.Code);
        Assert.Equal(BusinessErrorKind.Conflict, ex.Kind);
        Assert.Equal(["tai_khoan"], ex.Errors!.Keys);
        Assert.Equal(5, (await _service.GetPagingAsync(new PagingRequest(), CancellationToken.None)).meta.total);
    }

    [Fact]
    public async Task Create_allows_username_of_deleted_user()
    {
        var created = await _service.CreateAsync(NewUserWith("hv.daxoa"), CancellationToken.None);

        Assert.Equal("hv.daxoa", created.tai_khoan);
    }

    [Fact]
    public async Task Update_changes_allowed_fields_and_sets_modified_time()
    {
        var update = new NguoiDungUpdateDto { ho_ten = "Admin mới", vai_tro = "Admin", dang_hoat_dong = false };

        var updated = await _service.UpdateAsync(AdminId, update, CancellationToken.None);

        Assert.Equal("Admin mới", updated.ho_ten);
        Assert.Equal("admin", updated.tai_khoan);
        Assert.False(updated.dang_hoat_dong);
        Assert.Equal(Now, updated.ngay_chinh_sua);
        Assert.Equal("Admin mới", (await _service.GetByIdAsync(AdminId, CancellationToken.None)).ho_ten);
    }

    [Fact]
    public async Task Update_throws_not_found_for_missing_or_deleted_user()
    {
        var update = new NguoiDungUpdateDto { ho_ten = "X", vai_tro = "Trainee" };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), update, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(DeletedId, update, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_soft_deletes_user_once()
    {
        await _service.DeleteAsync(AdminId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(AdminId, CancellationToken.None));
        Assert.Equal(4, (await _service.GetPagingAsync(new PagingRequest(), CancellationToken.None)).meta.total);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(AdminId, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
