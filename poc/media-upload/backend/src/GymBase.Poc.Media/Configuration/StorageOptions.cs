namespace GymBase.Poc.Media.Configuration;

/// <summary>
/// Cấu hình lưu trữ tệp tin theo đúng tài liệu GymBase V1:
/// Hỗ trợ cả Local Storage (mặc định ban đầu) và S3 (khi mở rộng).
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Provider mặc định cho upload mới: "LOCAL" hoặc "S3"
    /// </summary>
    public string DefaultProvider { get; set; } = "LOCAL";

    /// <summary>
    /// Đường dẫn thư mục lưu trữ cục bộ (trên máy chủ hoặc Docker volume /app/uploads).
    /// </summary>
    public string LocalRoot { get; set; } = "uploads";

    /// <summary>
    /// Tên S3 Bucket khi sử dụng S3 provider.
    /// </summary>
    public string BucketName { get; set; } = "gymbase-media";

    /// <summary>
    /// AWS Region khi sử dụng S3 provider.
    /// </summary>
    public string Region { get; set; } = "ap-southeast-1";

    /// <summary>
    /// Endpoint tùy chỉnh khi dùng MinIO trong môi trường local/dev.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Bắt buộc true khi dùng MinIO.
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;

    /// <summary>
    /// Dung lượng upload tối đa cho phép (bytes). Mặc định 10MB theo tài liệu V1.
    /// </summary>
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Danh sách Content-Type cho phép upload.
    /// </summary>
    public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif"
    };

    /// <summary>
    /// Cho phép test API mà không bắt buộc JWT token trong môi trường POC.
    /// </summary>
    public bool AllowAnonymousForPoc { get; set; } = true;
}
