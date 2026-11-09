namespace GymBase.Poc.Media.Configuration;

/// <summary>
/// Cấu hình cho Media Upload Module theo Options Pattern.
/// </summary>
public sealed class MediaOptions
{
    public const string SectionName = "GymBase:Media";

    public string BucketName { get; set; } = "gymbase-media";
    public string Region { get; set; } = "ap-southeast-1";

    /// <summary>
    /// Endpoint tùy chỉnh khi dùng MinIO hoặc LocalStack trong môi trường local/dev/test.
    /// Ví dụ: "http://localhost:9000". Nếu null thì dùng AWS S3 chuẩn.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Bắt buộc true khi dùng MinIO (URL dạng http://localhost:9000/bucket-name/key).
    /// AWS S3 mặc định false (virtual hosted style).
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;

    /// <summary>
    /// Tùy chọn CloudFront CDN Base URL (ví dụ: "https://d123456.cloudfront.net" hoặc "https://cdn.gymbase.vn").
    /// Nếu để trống, server sẽ fallback sang Presigned GET hoặc S3 endpoint trực tiếp.
    /// </summary>
    public string? CdnBaseUrl { get; set; }

    /// <summary>
    /// Thời gian hết hạn của Presigned PUT/GET URL (phút).
    /// Khuyến nghị: 15 phút cho Presigned PUT.
    /// </summary>
    public int PresignExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Dung lượng upload tối đa cho phép (bytes). Mặc định 15MB.
    /// </summary>
    public long MaxUploadBytes { get; set; } = 15 * 1024 * 1024;

    /// <summary>
    /// Danh sách Content-Type được phép upload.
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
    /// Tùy chọn sử dụng InMemory storage cho môi trường test độc lập mà không cần MinIO/AWS.
    /// </summary>
    public bool UseInMemoryStorage { get; set; } = false;

    /// <summary>
    /// Cờ cho phép test API mà không bắt buộc JWT token.
    /// </summary>
    public bool AllowAnonymousForPoc { get; set; } = true;
}
