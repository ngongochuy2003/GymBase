using GymBase.Base.Entities;
using GymBase.Base.Paging;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Base;

/// <summary>Hành vi của repository giả dùng chung, phải khớp BaseRepository.</summary>
public class InMemoryBaseRepositoryTests
{
    private sealed class Tag : BaseEntity
    {
        public string ten { get; set; } = string.Empty;
    }

    private sealed class TagRepository(InMemoryStore<Tag> store) : InMemoryBaseRepository<Tag>(store);

    private readonly InMemoryStore<Tag> _store = new();

    [Fact]
    public async Task Changes_to_returned_entity_are_not_saved_until_update()
    {
        var repository = new TagRepository(_store);
        var tag = new Tag { ten = "a" };
        await repository.DLInsertAsync(tag, CancellationToken.None);

        var loaded = (await repository.DLGetByIdAsync(tag.Id, CancellationToken.None))!;
        loaded.ten = "b";
        Assert.Equal("a", (await repository.DLGetByIdAsync(tag.Id, CancellationToken.None))!.ten);

        Assert.True(await repository.DLUpdateAsync(loaded, CancellationToken.None));
        Assert.Equal("b", (await repository.DLGetByIdAsync(tag.Id, CancellationToken.None))!.ten);
    }

    [Fact]
    public async Task Search_is_ignored_when_entity_has_no_search_values()
    {
        var repository = new TagRepository(_store);
        await repository.DLInsertAsync(new Tag { ten = "a" }, CancellationToken.None);

        var page = await repository.DLGetPagingAsync(new PagingRequest { search = "zzz" }.Normalize(), CancellationToken.None);

        Assert.Equal(1, page.meta.total);
    }

    [Fact]
    public async Task Insert_rejects_duplicate_id_and_delete_removes_row()
    {
        var repository = new TagRepository(_store);
        var tag = new Tag();
        await repository.DLInsertAsync(tag, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.DLInsertAsync(tag, CancellationToken.None));
        Assert.True(await repository.DLDeleteAsync(tag.Id, CancellationToken.None));
        Assert.Null(await repository.DLGetByIdAsync(tag.Id, CancellationToken.None));
        Assert.False(await repository.DLUpdateAsync(tag, CancellationToken.None));
    }

    [Fact]
    public void Seed_runs_only_once()
    {
        _store.SeedOnce(() => [new Tag(), new Tag()]);
        _store.SeedOnce(() => [new Tag()]);

        Assert.Equal(2, _store.All().Count);
    }
}
