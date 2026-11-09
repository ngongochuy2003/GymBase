namespace GymBase.Poc.Media.Storage;

public sealed record StorageObjectMetadata(
    string Key,
    long SizeBytes,
    string ContentType,
    DateTime LastModified,
    string? ETag = null,
    string? CacheControl = null
);

public interface IMediaStorage
{
    Task<string> GetPresignedPutUrlAsync(
        string key,
        string contentType,
        TimeSpan expiry,
        CancellationToken ct = default);

    Task<string> GetPresignedGetUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);

    Task<StorageObjectMetadata?> HeadObjectAsync(
        string key,
        CancellationToken ct = default);

    Task<Stream> GetObjectStreamAsync(
        string key,
        CancellationToken ct = default);

    Task PutObjectAsync(
        string key,
        Stream data,
        string contentType,
        string cacheControl,
        CancellationToken ct = default);

    Task DeleteObjectAsync(
        string key,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string key,
        CancellationToken ct = default);
}
