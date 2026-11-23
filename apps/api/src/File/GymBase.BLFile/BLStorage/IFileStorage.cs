namespace GymBase.BLFile.BLStorage;

public record StoredObjectInfo(long Length, string ContentType);

/// <summary>
/// Form upload đã ký (presigned POST). App gửi <c>multipart/form-data</c> tới <see cref="Url"/> với mọi <see cref="Fields"/> rồi tới
/// trường file ở cuối. Policy đã ký ràng buộc key, <c>Content-Type</c> và kích thước từ 1 byte đến <c>maxBytes</c>.
/// </summary>
public sealed record PresignedUpload(string Url, IReadOnlyDictionary<string, string> Fields);

/// <summary>
/// Hợp đồng trừu tượng hóa tầng lưu trữ tệp tin nhị phân.
/// Triển khai cho cả Local File Storage (chỉ dùng cho test) và Amazon S3 / MinIO.
/// </summary>
public interface IFileStorage
{
    string ProviderName { get; }

    /// <summary>Có ký được presigned URL không. Local thì không: app phải upload thẳng lên MinIO/S3.</summary>
    bool SupportsPresign { get; }

    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string key, CancellationToken ct = default);
    Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Presigned POST với policy: đúng key, đúng <c>Content-Type</c>, kích thước từ 1 đến <paramref name="maxBytes"/>. Dùng POST chứ không
    /// dùng presigned PUT vì PUT không ép được kích thước (đã thử trên MinIO: body lớn hơn khai báo vẫn được nhận). Không ràng buộc đúng một
    /// số byte vì app không biết chính xác dung lượng sau khi nén; kích thước thật lấy từ storage lúc confirm.
    /// </summary>
    PresignedUpload CreatePresignedUpload(string key, string contentType, long maxBytes, TimeSpan expiry);

    /// <summary>Presigned GET; ai có URL đều đọc được tới khi hết hạn nên hạn phải ngắn.</summary>
    string CreatePresignedGetUrl(string key, TimeSpan expiry);

    /// <summary>Sao chép object trong cùng storage (phía server, không kéo dữ liệu về API).</summary>
    Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct = default);

    /// <summary>Đọc tối đa <paramref name="count"/> byte đầu của object, để kiểm magic bytes.</summary>
    Task<byte[]> ReadHeadAsync(string key, int count, CancellationToken ct = default);
}
