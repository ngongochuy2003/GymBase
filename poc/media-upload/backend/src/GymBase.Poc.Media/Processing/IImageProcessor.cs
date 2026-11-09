namespace GymBase.Poc.Media.Processing;

public sealed record ProcessedVariant(
    string VariantName,
    byte[] WebpBytes,
    int Width,
    int Height
);

public sealed record ProcessedImageResult(
    string ContentHash,
    IReadOnlyList<ProcessedVariant> Variants,
    int OriginalWidth,
    int OriginalHeight,
    string Blurhash,
    long TotalVariantsSizeBytes
);

public interface IImageProcessor
{
    /// <summary>
    /// Kiểm tra magic bytes trong stream để xác nhận đúng là file ảnh,
    /// chống trường hợp client giả mạo header Content-Type.
    /// </summary>
    void ValidateMagicBytes(Stream stream, string claimedContentType);

    /// <summary>
    /// Giải mã, kiểm tra chống decompression bomb, resize các biến thể (thumb, medium, large),
    /// encode sang WebP, tính content hash và tạo blurhash.
    /// </summary>
    Task<ProcessedImageResult> ProcessVariantsAsync(
        Stream stream,
        string purpose,
        CancellationToken ct = default);
}
