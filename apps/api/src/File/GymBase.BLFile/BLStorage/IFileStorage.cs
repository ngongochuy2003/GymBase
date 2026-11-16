namespace GymBase.BLFile.BLStorage;

public record StoredObjectInfo(long Length, string ContentType);

/// <summary>
/// Hợp đồng trừu tượng hóa tầng lưu trữ tệp tin nhị phân.
/// Triển khai cho cả Local File Storage (giai đoạn đầu) và Amazon S3 / MinIO (giai đoạn sau).
/// </summary>
public interface IFileStorage
{
    string ProviderName { get; }
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string key, CancellationToken ct = default);
    Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
