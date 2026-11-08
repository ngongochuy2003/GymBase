using GymBase.AMISBase.DL.Contract.Entities;
using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.AMISBase.DL.Mysql.InMemory;

namespace GymBase.UnitTests.AMISBase;

/// <summary>Hành vi của DL mock dùng chung mà DL SQL sau này cũng phải giữ.</summary>
public class InMemoryBaseDLTests
{
    private sealed class Tag : BaseEntity
    {
        public string ten { get; set; } = string.Empty;
    }

    private sealed class TagDL(InMemoryStore<Tag> store) : InMemoryBaseDL<Tag>(store);

    private readonly InMemoryStore<Tag> _store = new();

    [Fact]
    public async Task Changes_to_returned_entity_are_not_saved_until_update()
    {
        var dl = new TagDL(_store);
        var tag = new Tag { ten = "a" };
        await dl.InsertAsync(tag, CancellationToken.None);

        var loaded = (await dl.GetByIdAsync(tag.Id, CancellationToken.None))!;
        loaded.ten = "b";
        Assert.Equal("a", (await dl.GetByIdAsync(tag.Id, CancellationToken.None))!.ten);

        Assert.True(await dl.UpdateAsync(loaded, CancellationToken.None));
        Assert.Equal("b", (await dl.GetByIdAsync(tag.Id, CancellationToken.None))!.ten);
    }

    [Fact]
    public async Task Search_is_ignored_when_entity_has_no_search_values()
    {
        var dl = new TagDL(_store);
        await dl.InsertAsync(new Tag { ten = "a" }, CancellationToken.None);

        var page = await dl.GetPagingAsync(new PagingRequest { Search = "zzz" }.Normalize(), CancellationToken.None);

        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task Insert_rejects_duplicate_id_and_delete_removes_row()
    {
        var dl = new TagDL(_store);
        var tag = new Tag();
        await dl.InsertAsync(tag, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dl.InsertAsync(tag, CancellationToken.None));
        Assert.True(await dl.DeleteAsync(tag.Id, CancellationToken.None));
        Assert.Null(await dl.GetByIdAsync(tag.Id, CancellationToken.None));
        Assert.False(await dl.UpdateAsync(tag, CancellationToken.None));
    }

    [Fact]
    public void Seed_runs_only_once()
    {
        _store.SeedOnce(() => [new Tag(), new Tag()]);
        _store.SeedOnce(() => [new Tag()]);

        Assert.Equal(2, _store.All().Count);
    }
}
