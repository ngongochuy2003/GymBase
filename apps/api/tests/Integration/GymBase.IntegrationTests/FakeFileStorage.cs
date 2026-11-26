using GymBase.BLFile.BLStorage;

namespace GymBase.IntegrationTests;

/// <summary>
/// Kho lưu trữ giả cho test: giữ các object trong bộ nhớ (để test upload hai bước đặt file vào key tạm như app làm) và nhớ các khóa đã bị
/// xóa để test kiểm tra.
/// </summary>
internal sealed class FakeFileStorage : IFileStorage
{
    public List<string> Deleted { get; } = [];

    public Dictionary<string, (byte[] Data, string ContentType)> Objects { get; } = new();

    public string ProviderName => "FAKE";

    public bool SupportsPresign => true;

    public PresignedUpload CreatePresignedUpload(string key, string contentType, long maxBytes, TimeSpan expiry) =>
        new("https://fake.test/upload", new Dictionary<string, string> { ["key"] = key, ["Content-Type"] = contentType });

    public string CreatePresignedGetUrl(string key, TimeSpan expiry) => $"https://fake.test/get/{key}?exp={(int)expiry.TotalMinutes}";

    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct = default)
    {
        Objects[destinationKey] = Objects[sourceKey];
        return Task.CompletedTask;
    }

    public Task<byte[]> ReadHeadAsync(string key, int count, CancellationToken ct = default) =>
        Task.FromResult(Objects.TryGetValue(key, out var o) ? o.Data.Take(count).ToArray() : []);

    public Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        Objects[key] = (ms.ToArray(), contentType);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream(Objects.TryGetValue(key, out var o) ? o.Data : []));

    public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(Objects.TryGetValue(key, out var o) ? new StoredObjectInfo(o.Data.Length, o.ContentType) : null);

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        Deleted.Add(key);
        Objects.Remove(key);
        return Task.CompletedTask;
    }
}

internal sealed class FakeStorageResolver(FakeFileStorage storage) : IStorageResolver
{
    public IFileStorage Resolve(string providerName) => storage;

    public IFileStorage GetDefault() => storage;
}
