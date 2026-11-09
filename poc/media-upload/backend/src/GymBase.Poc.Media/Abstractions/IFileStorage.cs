namespace GymBase.Poc.Media.Abstractions;

public record StoredObjectInfo(long Length, string ContentType);

/// <summary>
/// Hợp đồng trừu tượng hóa tầng lưu trữ tệp tin nhị phân.
/// Triển khai cho cả Local File Storage (giai đoạn đầu) và Amazon S3 / MinIO (giai đoạn sau).
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Định danh provider: "LOCAL" hoặc "S3"
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Lưu luồng dữ liệu vào storage theo storage key.
    /// </summary>
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Mở luồng đọc dữ liệu từ storage.
    /// </summary>
    Task<Stream> OpenReadAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Lấy thông tin kích thước và mime-type của đối tượng.
    /// </summary>
    Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Xóa đối tượng khỏi storage.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken ct = default);
}
