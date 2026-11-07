using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.BLSystem.Users;
using GymBase.DLSystem.Mysql;

namespace GymBase.UnitTests.System;

/// <summary>Kiểm tra UserBL trên dữ liệu mock của UserDL (5 người dùng còn hiệu lực, 1 đã xóa).</summary>
public class UserBLTests
{
    private static readonly Guid AdminId = Guid.Parse("0193f0a0-0000-7000-8000-000000000001");
    private static readonly Guid DeletedId = Guid.Parse("0193f0a0-0000-7000-8000-000000000006");

    private readonly UserBL _bl = new(new UserDL());

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
}
