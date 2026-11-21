using GymBase.BLFile.BLStorage;

namespace GymBase.IntegrationTests;

/// <summary>Kho lưu trữ giả cho test: không ghi gì, chỉ nhớ các khóa đã bị xóa để test kiểm tra.</summary>
internal sealed class FakeFileStorage : IFileStorage
{
    public List<string> Deleted { get; } = [];

    public string ProviderName => "FAKE";

    public Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default) => Task.CompletedTask;

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream());

    public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default) =>
        Task.FromResult<StoredObjectInfo?>(null);

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        Deleted.Add(key);
        return Task.CompletedTask;
    }
}

internal sealed class FakeStorageResolver(FakeFileStorage storage) : IStorageResolver
{
    public IFileStorage Resolve(string providerName) => storage;

    public IFileStorage GetDefault() => storage;
}
