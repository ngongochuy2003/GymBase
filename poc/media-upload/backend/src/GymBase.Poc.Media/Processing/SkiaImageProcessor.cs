using System.Security.Cryptography;
using SkiaSharp;

namespace GymBase.Poc.Media.Processing;

public sealed class SkiaImageProcessor : IImageProcessor
{
    private const int MaxDimensionLimit = 8192; // Chống decompression bomb
    private const long MaxTotalPixels = 36_000_000; // ~36 Megapixels

    // Target sizes cho các biến thể
    private const int ThumbMax = 200;
    private const int MediumMax = 720;
    private const int LargeMax = 1440;
    private const int WebpQuality = 82;

    public void ValidateMagicBytes(Stream stream, string claimedContentType)
    {
        if (stream == null || stream.Length < 12)
        {
            throw new InvalidDataException("Dữ liệu ảnh rỗng hoặc không đủ kích thước header tối thiểu.");
        }

        var originalPosition = stream.Position;
        stream.Position = 0;

        Span<byte> header = stackalloc byte[16];
        var bytesRead = stream.Read(header);
        stream.Position = originalPosition;

        if (bytesRead < 12)
        {
            throw new InvalidDataException("Không thể đọc đủ 12 bytes header.");
        }

        var isGenuineImage = IsJpeg(header) || IsPng(header) || IsWebp(header) || IsHeic(header);
        if (!isGenuineImage)
        {
            throw new InvalidDataException("Nội dung file không khớp với magic bytes của ảnh hợp lệ (JPEG, PNG, WebP, HEIC).");
        }
    }

    public Task<ProcessedImageResult> ProcessVariantsAsync(
        Stream stream,
        string purpose,
        CancellationToken ct = default)
    {
        stream.Position = 0;

        // 1. Tính SHA-256 hash của luồng dữ liệu gốc để làm Content Hash (Deduplication)
        string contentHash;
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(stream);
            contentHash = Convert.ToHexStringLower(hashBytes);
        }

        stream.Position = 0;

        // 2. Mở Codec để đọc metadata và kích thước trước khi giải mã toàn bộ pixels (Anti Decompression-Bomb)
        using var skData = SKData.Create(stream);
        if (skData == null)
            throw new InvalidDataException("Không thể nạp dữ liệu ảnh vào bộ nhớ.");

        using var codec = SKCodec.Create(skData);
        if (codec == null)
            throw new InvalidDataException("Định dạng ảnh bị hỏng hoặc không được hỗ trợ giải mã.");

        var width = codec.Info.Width;
        var height = codec.Info.Height;

        if (width <= 0 || height <= 0)
            throw new InvalidDataException("Kích thước ảnh không hợp lệ.");

        if (width > MaxDimensionLimit || height > MaxDimensionLimit || (long)width * height > MaxTotalPixels)
        {
            throw new InvalidOperationException(
                $"Kích thước ảnh ({width}x{height}) vượt quá giới hạn an toàn {MaxDimensionLimit}px / 36MP (chống Decompression Bomb).");
        }

        // 3. Giải mã toàn bộ Bitmap và xử lý xoay EXIF nếu có
        using var rawBitmap = SKBitmap.Decode(codec);
        if (rawBitmap == null)
            throw new InvalidDataException("Không thể giải mã dữ liệu điểm ảnh.");

        using var orientedBitmap = ApplyExifOrientation(rawBitmap, codec.EncodedOrigin);
        var origW = orientedBitmap.Width;
        var origH = orientedBitmap.Height;

        // 4. Sinh Blurhash từ ảnh thu nhỏ 32x32
        var blurhash = GenerateBlurhash(orientedBitmap);

        // 5. Sinh 3 biến thể ảnh: thumb (200px), medium (720px), large (1440px) dạng WebP
        var variants = new List<ProcessedVariant>(3);

        // 5.1 Thumb
        var thumb = CreateVariant(orientedBitmap, "thumb", ThumbMax);
        variants.Add(thumb);

        // 5.2 Medium
        var medium = CreateVariant(orientedBitmap, "medium", MediumMax);
        variants.Add(medium);

        // 5.3 Large
        var large = CreateVariant(orientedBitmap, "large", LargeMax);
        variants.Add(large);

        var totalBytes = variants.Sum(v => (long)v.WebpBytes.Length);

        var result = new ProcessedImageResult(
            ContentHash: contentHash,
            Variants: variants,
            OriginalWidth: origW,
            OriginalHeight: origH,
            Blurhash: blurhash,
            TotalVariantsSizeBytes: totalBytes
        );

        return Task.FromResult(result);
    }

    private static ProcessedVariant CreateVariant(SKBitmap source, string variantName, int maxDimension)
    {
        var (targetW, targetH) = CalculateAspectFitDimensions(source.Width, source.Height, maxDimension);

        SKBitmap targetBitmap;
        bool shouldDispose = false;

        if (targetW == source.Width && targetH == source.Height)
        {
            targetBitmap = source;
        }
        else
        {
            targetBitmap = source.Resize(new SKImageInfo(targetW, targetH, SKColorType.Rgba8888), SKFilterQuality.High) ?? source;
            if (targetBitmap != source)
            {
                shouldDispose = true;
            }
        }

        try
        {
            using var image = SKImage.FromBitmap(targetBitmap);
            using var webpData = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);

            return new ProcessedVariant(
                VariantName: variantName,
                WebpBytes: webpData.ToArray(),
                Width: targetW,
                Height: targetH
            );
        }
        finally
        {
            if (shouldDispose)
            {
                targetBitmap.Dispose();
            }
        }
    }

    private static (int Width, int Height) CalculateAspectFitDimensions(int origW, int origH, int maxDim)
    {
        // Không upscale ảnh nhỏ hơn maxDim
        if (origW <= maxDim && origH <= maxDim)
            return (origW, origH);

        double ratio = (double)origW / origH;
        if (origW >= origH)
        {
            var targetW = maxDim;
            var targetH = Math.Max(1, (int)Math.Round(targetW / ratio));
            return (targetW, targetH);
        }
        else
        {
            var targetH = maxDim;
            var targetW = Math.Max(1, (int)Math.Round(targetH * ratio));
            return (targetW, targetH);
        }
    }

    private static string GenerateBlurhash(SKBitmap source)
    {
        const int bhWidth = 32;
        const int bhHeight = 32;

        using var small = source.Resize(new SKImageInfo(bhWidth, bhHeight, SKColorType.Rgb888x), SKFilterQuality.Medium);
        if (small == null) return "L6PZfSi_.AyE_3t7t7R**0o#DgR4"; // Fallback generic neutral hash

        var rgbBytes = new byte[bhWidth * bhHeight * 3];
        var span = small.GetPixelSpan();
        var bytesPerPixel = small.BytesPerPixel;

        for (var i = 0; i < bhWidth * bhHeight; i++)
        {
            var offset = i * bytesPerPixel;
            rgbBytes[i * 3 + 0] = span[offset + 0]; // R
            rgbBytes[i * 3 + 1] = span[offset + 1]; // G
            rgbBytes[i * 3 + 2] = span[offset + 2]; // B
        }

        return BlurhashEncoder.Encode(rgbBytes, bhWidth, bhHeight, 4, 3);
    }

    private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        return origin switch
        {
            SKEncodedOrigin.RightTop => RotateBitmap(bitmap, 90),
            SKEncodedOrigin.BottomRight => RotateBitmap(bitmap, 180),
            SKEncodedOrigin.LeftBottom => RotateBitmap(bitmap, 270),
            _ => bitmap
        };
    }

    private static SKBitmap RotateBitmap(SKBitmap source, float degrees)
    {
        var isSwap = Math.Abs(degrees - 90) < 0.1f || Math.Abs(degrees - 270) < 0.1f;
        var newW = isSwap ? source.Height : source.Width;
        var newH = isSwap ? source.Width : source.Height;

        var rotated = new SKBitmap(newW, newH);
        using (var canvas = new SKCanvas(rotated))
        {
            canvas.Translate(newW / 2.0f, newH / 2.0f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-source.Width / 2.0f, -source.Height / 2.0f);
            canvas.DrawBitmap(source, 0, 0);
        }
        return rotated;
    }

    private static bool IsJpeg(ReadOnlySpan<byte> h) =>
        h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;

    private static bool IsPng(ReadOnlySpan<byte> h) =>
        h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 &&
        h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A;

    private static bool IsWebp(ReadOnlySpan<byte> h) =>
        h.Length >= 12 &&
        h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && // 'RIFF'
        h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50; // 'WEBP'

    private static bool IsHeic(ReadOnlySpan<byte> h) =>
        h.Length >= 8 &&
        h[4] == 0x66 && h[5] == 0x74 && h[6] == 0x79 && h[7] == 0x70; // 'ftyp'
}
