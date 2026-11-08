using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.AMISBase.DL.Mysql.InMemory;
using GymBase.BLSystem.NguoiDung;
using GymBase.DLSystem.Mysql;
using GymBase.Model.Entities;

namespace GymBase.UnitTests.System;

/// <summary>
/// Kiểm tra UserBL (CRUD lấy từ BaseBL) trên dữ liệu mock của UserDL: 5 người dùng còn hiệu lực, 1 đã xóa.
/// Mỗi test có store riêng nên thêm/sửa/xóa không ảnh hưởng test khác.
/// </summary>
public class UserBLTests
{
    private static readonly Guid AdminId = Guid.Parse("0193f0a0-0000-7000-8000-000000000001");
    private static readonly Guid DeletedId = Guid.Parse("0193f0a0-0000-7000-8000-000000000006");
    private static readonly DateTime Now = new(2026, 11, 20, 8, 30, 0, DateTimeKind.Utc);

    private readonly UserBL _bl = new(new UserDL(new InMemoryStore<nguoi_dung>()), new FixedTimeProvider(Now));

    private static readonly NguoiDungCreateDto NewUser = new()
    {
        tai_khoan = " hv.moi ",
        ho_ten = "Học viên mới",
        email = "moi@gymbase.local",
        vai_tro = "Trainee",
    };

    [Fact]
    public async Task Paging_hides_deleted_users_and_sorts_newest_first()
    {
        var result = await _bl.GetPagingAsync(new PagingRequest(), CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
        Assert.DoesNotContain(result.Items, u => u.Id == DeletedId);
        Assert.Equal("hv.phuong", result.Items[0].tai_khoan);
        Assert.Equal("admin", result.Items[^1].tai_khoan);
    }

    [Fact]
    public async Task Paging_splits_pages()
    {
        var result = await _bl.GetPagingAsync(new PagingRequest { Page = 2, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal("pt.lan", result.Items[0].tai_khoan);
    }

    [Fact]
    public async Task Search_matches_name_case_insensitively()
    {
        var result = await _bl.GetPagingAsync(new PagingRequest { Search = "  PT. " }, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, u => Assert.Equal("Trainer", u.vai_tro));
    }

    [Theory]
    [InlineData(0, 0, 1, PagingRequest.DefaultPageSize)]
    [InlineData(-3, 500, 1, PagingRequest.MaxPageSize)]
    public async Task Invalid_paging_values_are_normalized(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var result = await _bl.GetPagingAsync(new PagingRequest { Page = page, PageSize = pageSize }, CancellationToken.None);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
    }

    [Fact]
    public async Task GetById_returns_user()
    {
        var user = await _bl.GetByIdAsync(AdminId, CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal("admin", user.tai_khoan);
    }

    [Fact]
    public async Task GetById_returns_null_for_missing_or_deleted_user()
    {
        Assert.Null(await _bl.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
        Assert.Null(await _bl.GetByIdAsync(DeletedId, CancellationToken.None));
    }

    [Fact]
    public async Task Create_assigns_uuidv7_and_utc_audit_then_is_readable()
    {
        var created = await _bl.CreateAsync(NewUser, CancellationToken.None);

        Assert.Equal(7, created.Id.Version);
        Assert.Equal("hv.moi", created.tai_khoan);
        Assert.Equal(Now, created.ngay_tao);
        Assert.Null(created.ngay_chinh_sua);
        Assert.True(created.dang_hoat_dong);

        var page = await _bl.GetPagingAsync(new PagingRequest(), CancellationToken.None);
        Assert.Equal(6, page.TotalCount);
        Assert.Equal(created.Id, page.Items[0].Id);
    }

    [Fact]
    public async Task Update_changes_allowed_fields_and_sets_modified_time()
    {
        var update = new NguoiDungUpdateDto { ho_ten = "Admin mới", vai_tro = "Admin", dang_hoat_dong = false };

        var updated = await _bl.UpdateAsync(AdminId, update, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Admin mới", updated.ho_ten);
        Assert.Equal("admin", updated.tai_khoan);
        Assert.False(updated.dang_hoat_dong);
        Assert.Equal(Now, updated.ngay_chinh_sua);
        Assert.Equal("Admin mới", (await _bl.GetByIdAsync(AdminId, CancellationToken.None))!.ho_ten);
    }

    [Fact]
    public async Task Update_returns_null_for_missing_or_deleted_user()
    {
        var update = new NguoiDungUpdateDto { ho_ten = "X", vai_tro = "Trainee" };

        Assert.Null(await _bl.UpdateAsync(Guid.NewGuid(), update, CancellationToken.None));
        Assert.Null(await _bl.UpdateAsync(DeletedId, update, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_soft_deletes_user_once()
    {
        Assert.True(await _bl.DeleteAsync(AdminId, CancellationToken.None));

        Assert.Null(await _bl.GetByIdAsync(AdminId, CancellationToken.None));
        Assert.Equal(4, (await _bl.GetPagingAsync(new PagingRequest(), CancellationToken.None)).TotalCount);
        Assert.False(await _bl.DeleteAsync(AdminId, CancellationToken.None));
        Assert.False(await _bl.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
