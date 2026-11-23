using SkiaSharp;

namespace GymBase.BLFile.BLProcessing;

public sealed class SkiaImageProcessor : IImageProcessor
{
    private const int MaxDimensionLimit = 8192;
    private const long MaxTotalPixels = 36_000_000;
    private const int JpegQuality = 92;
    private const int WebpQuality = 92;

    public Task<ProcessedImage> XuLyAsync(Stream source, string contentType, CancellationToken ct = default)
    {
        var format = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => SKEncodedImageFormat.Jpeg,
            "image/png" => SKEncodedImageFormat.Png,
            "image/webp" => SKEncodedImageFormat.Webp,
            _ => throw new InvalidDataException($"Định dạng {contentType} chưa hỗ trợ xử lý."),
        };

        using var data = SKData.Create(source) ?? throw new InvalidDataException("Không đọc được dữ liệu ảnh.");
        using var codec = SKCodec.Create(data) ?? throw new InvalidDataException("Ảnh hỏng hoặc không giải mã được.");

        var (width, height) = (codec.Info.Width, codec.Info.Height);
        if (width <= 0 || height <= 0)
        {
            throw new InvalidDataException("Kích thước ảnh không hợp lệ.");
        }

        // Kiểm kích thước từ header trước khi cấp bộ nhớ cho điểm ảnh.
        if (width > MaxDimensionLimit || height > MaxDimensionLimit || (long)width * height > MaxTotalPixels)
        {
            throw new ImageTooLargeException($"Ảnh {width}x{height} vượt giới hạn {MaxDimensionLimit}px hoặc 36MP.");
        }

        ct.ThrowIfCancellationRequested();
        using var decoded = SKBitmap.Decode(codec) ?? throw new InvalidDataException("Không giải mã được điểm ảnh.");
        using var oriented = ApplyOrientation(decoded, codec.EncodedOrigin);

        var blurhash = GenerateBlurhash(oriented);
        using var image = SKImage.FromBitmap(oriented);
        using var encoded = image.Encode(format, format == SKEncodedImageFormat.Webp ? WebpQuality : JpegQuality)
                            ?? throw new InvalidDataException("Không mã hóa lại được ảnh.");

        return Task.FromResult(new ProcessedImage(encoded.ToArray(), oriented.Width, oriented.Height, blurhash));
    }

    private static string GenerateBlurhash(SKBitmap source)
    {
        const int size = 32;
        using var small = source.Resize(new SKImageInfo(size, size, SKColorType.Rgba8888), SKFilterQuality.Medium)
                          ?? throw new InvalidDataException("Không thu nhỏ được ảnh để tính blurhash.");

        var rgb = new byte[size * size * 3];
        var pixels = small.GetPixelSpan();
        var bytesPerPixel = small.BytesPerPixel;
        for (var i = 0; i < size * size; i++)
        {
            var offset = i * bytesPerPixel;
            rgb[i * 3] = pixels[offset];
            rgb[i * 3 + 1] = pixels[offset + 1];
            rgb[i * 3 + 2] = pixels[offset + 2];
        }

        return BlurhashEncoder.Encode(rgb, size, size, 4, 3);
    }

    /// <summary>Xoay/lật theo EXIF để ảnh hiển thị đúng chiều sau khi metadata bị bỏ. Trả chính bitmap gốc khi không cần đổi.</summary>
    private static SKBitmap ApplyOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (origin is SKEncodedOrigin.TopLeft or SKEncodedOrigin.Default)
        {
            return Copy(bitmap);
        }

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var rotated = new SKBitmap(swap ? bitmap.Height : bitmap.Width, swap ? bitmap.Width : bitmap.Height);
        using var canvas = new SKCanvas(rotated);
        var (w, h) = (bitmap.Width, bitmap.Height);
        canvas.SetMatrix(origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix { ScaleX = -1, ScaleY = 1, TransX = w, Persp2 = 1 },
            SKEncodedOrigin.BottomRight => new SKMatrix { ScaleX = -1, ScaleY = -1, TransX = w, TransY = h, Persp2 = 1 },
            SKEncodedOrigin.BottomLeft => new SKMatrix { ScaleX = 1, ScaleY = -1, TransY = h, Persp2 = 1 },
            SKEncodedOrigin.LeftTop => new SKMatrix { SkewX = 1, SkewY = 1, Persp2 = 1 },
            SKEncodedOrigin.RightTop => new SKMatrix { SkewX = -1, SkewY = 1, TransX = h, Persp2 = 1 },
            SKEncodedOrigin.RightBottom => new SKMatrix { SkewX = -1, SkewY = -1, TransX = h, TransY = w, Persp2 = 1 },
            SKEncodedOrigin.LeftBottom => new SKMatrix { SkewX = 1, SkewY = -1, TransY = w, Persp2 = 1 },
            _ => SKMatrix.Identity,
        });
        canvas.DrawBitmap(bitmap, 0, 0);
        return rotated;
    }

    private static SKBitmap Copy(SKBitmap source) => source.Copy() ?? throw new InvalidDataException("Không sao chép được ảnh.");
}
