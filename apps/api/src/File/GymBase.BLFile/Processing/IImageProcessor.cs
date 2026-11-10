namespace GymBase.BLFile.Processing;

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
    void ValidateMagicBytes(Stream stream, string claimedContentType);
    Task<ProcessedImageResult> ProcessVariantsAsync(Stream stream, string purpose, CancellationToken ct = default);
}
