namespace GymBase.Poc.Media.Contracts;

/// <summary>
/// Mục đích sử dụng ảnh trong hệ thống GymBase V1.
/// Mỗi mục đích có giới hạn kích thước và chính sách lưu trữ tương ứng.
/// </summary>
public static class MediaPurpose
{
    public const string Avatar = "avatar";
    public const string GymPhoto = "gym_photo";
    public const string ProgressPhoto = "progress_photo";
    public const string PostImage = "post_image";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Avatar,
        GymPhoto,
        ProgressPhoto,
        PostImage
    };

    public static bool IsValid(string? purpose) =>
        !string.IsNullOrWhiteSpace(purpose) && All.Contains(purpose);

    /// <summary>
    /// Giới hạn dung lượng tối đa (bytes) cho từng mục đích upload.
    /// Avatar: 5MB, Progress Photo: 12MB, Gym Photo / Post: 15MB.
    /// </summary>
    public static long GetMaxSizeBytes(string purpose) =>
        purpose.ToLowerInvariant() switch
        {
            Avatar => 5 * 1024 * 1024,
            ProgressPhoto => 12 * 1024 * 1024,
            _ => 15 * 1024 * 1024
        };

    /// <summary>
    /// Đề xuất Storage Class trên S3:
    /// - Avatar, GymPhoto, PostImage: Standard (truy cập thường xuyên).
    /// - ProgressPhoto: Intelligent-Tiering (ảnh tiến độ tập luyện cũ ít truy cập sau 30-90 ngày).
    /// </summary>
    public static string GetRecommendedStorageClass(string purpose) =>
        purpose.ToLowerInvariant() switch
        {
            ProgressPhoto => "INTELLIGENT_TIERING",
            _ => "STANDARD"
        };
}
