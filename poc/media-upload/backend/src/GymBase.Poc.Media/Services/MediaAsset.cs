namespace GymBase.Poc.Media.Services;

/// <summary>
/// Thực thể Media Asset lưu trữ thông tin ảnh sau khi xác nhận.
/// Tương ứng với bảng MySQL `media_asset` trong thiết kế cơ sở dữ liệu GymBase V1.
/// </summary>
public sealed class MediaAsset
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Purpose { get; init; } = string.Empty;
    public string ContentHash { get; init; } = string.Empty;
    public string BaseKey { get; init; } = string.Empty;
    public string ThumbKey { get; init; } = string.Empty;
    public string MediumKey { get; init; } = string.Empty;
    public string LargeKey { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public long TotalSizeBytes { get; init; }
    public string Blurhash { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public string StorageClass { get; init; } = "STANDARD";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
