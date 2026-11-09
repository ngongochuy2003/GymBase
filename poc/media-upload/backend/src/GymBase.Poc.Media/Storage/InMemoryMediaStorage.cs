using System.Collections.Concurrent;

namespace GymBase.Poc.Media.Storage;

/// <summary>
/// Triển khai IMediaStorage trong bộ nhớ (In-Memory).
/// Dùng cho integration test và chạy POC độc lập mà không cần MinIO/AWS S3.
/// </summary>
public sealed class InMemoryMediaStorage : IMediaStorage
{
    private sealed record StoredObject(
        byte[] Bytes,
        string ContentType,
        string CacheControl,
        DateTime CreatedAt
    );

    private readonly ConcurrentDictionary<string, StoredObject> _store = new(StringComparer.Ordinal);

    public Task<string> GetPresignedPutUrlAsync(
        string key,
        string contentType,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var mockUrl = $"http://localhost:5000/mock-s3-upload/{Uri.EscapeDataString(key)}";
        return Task.FromResult(mockUrl);
    }

    public Task<string> GetPresignedGetUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var mockUrl = $"http://localhost:5000/mock-s3-get/{Uri.EscapeDataString(key)}";
        return Task.FromResult(mockUrl);
    }

    public Task<StorageObjectMetadata?> HeadObjectAsync(string key, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(key, out var obj))
        {
            return Task.FromResult<StorageObjectMetadata?>(null);
        }

        var meta = new StorageObjectMetadata(
            Key: key,
            SizeBytes: obj.Bytes.Length,
            ContentType: obj.ContentType,
            LastModified: obj.CreatedAt,
            CacheControl: obj.CacheControl
        );
        return Task.FromResult<StorageObjectMetadata?>(meta);
    }

    public Task<Stream> GetObjectStreamAsync(string key, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(key, out var obj))
        {
            throw new FileNotFoundException($"Object not found in memory storage: {key}");
        }

        return Task.FromResult<Stream>(new MemoryStream(obj.Bytes, writable: false));
    }

    public Task PutObjectAsync(
        string key,
        Stream data,
        string contentType,
        string cacheControl,
        CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        var bytes = ms.ToArray();

        _store[key] = new StoredObject(bytes, contentType, cacheControl, DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task DeleteObjectAsync(string key, CancellationToken ct = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(_store.ContainsKey(key));
    }

    // Tiện ích hỗ trợ test trực tiếp
    public void DirectPut(string key, byte[] bytes, string contentType, string cacheControl = "public, max-age=31536000")
    {
        _store[key] = new StoredObject(bytes, contentType, cacheControl, DateTime.UtcNow);
    }

    public byte[]? DirectGet(string key)
    {
        return _store.TryGetValue(key, out var obj) ? obj.Bytes : null;
    }

    public void Clear() => _store.Clear();
}
