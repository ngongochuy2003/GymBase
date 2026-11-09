using FluentAssertions;
using GymBase.Poc.Media.Processing;
using SkiaSharp;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class SkiaImageProcessorTests
{
    private readonly SkiaImageProcessor _processor = new();

    private static byte[] CreateTestJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.CornflowerBlue);

        using var paint = new SKPaint { Color = SKColors.OrangeRed, StrokeWidth = 5 };
        canvas.DrawCircle(width / 2f, height / 2f, Math.Min(width, height) / 4f, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }

    [Fact]
    public void ValidateMagicBytes_WithValidJpeg_ShouldPass()
    {
        var bytes = CreateTestJpeg(100, 100);
        using var stream = new MemoryStream(bytes);

        var act = () => _processor.ValidateMagicBytes(stream, "image/jpeg");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateMagicBytes_WithTextFile_DisguisedAsJpeg_ShouldThrowInvalidDataException()
    {
        var fakeBytes = System.Text.Encoding.UTF8.GetBytes("<html><body>This is a phishing page disguised as an image</body></html>");
        using var stream = new MemoryStream(fakeBytes);

        var act = () => _processor.ValidateMagicBytes(stream, "image/jpeg");

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*không khớp với magic bytes của ảnh hợp lệ*");
    }

    [Fact]
    public void ValidateMagicBytes_WithCorruptedShortHeader_ShouldThrowInvalidDataException()
    {
        var shortBytes = new byte[] { 0xFF, 0xD8 }; // Chỉ 2 bytes
        using var stream = new MemoryStream(shortBytes);

        var act = () => _processor.ValidateMagicBytes(stream, "image/jpeg");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public async Task ProcessVariantsAsync_ShouldGenerateThreeVariants_AndValidBlurhash()
    {
        // Tạo ảnh mẫu 1920x1080 (tỷ lệ 16:9)
        var bytes = CreateTestJpeg(1920, 1080);
        using var stream = new MemoryStream(bytes);

        var result = await _processor.ProcessVariantsAsync(stream, "progress_photo");

        result.Should().NotBeNull();
        result.OriginalWidth.Should().Be(1920);
        result.OriginalHeight.Should().Be(1080);
        result.ContentHash.Should().HaveLength(64); // SHA-256
        result.Blurhash.Should().NotBeNullOrWhiteSpace();
        result.Blurhash.Length.Should().BeGreaterThan(10);

        result.Variants.Should().HaveCount(3);

        // Biến thể Thumb: max 200px
        var thumb = result.Variants.First(v => v.VariantName == "thumb");
        thumb.Width.Should().Be(200);
        thumb.Height.Should().BeInRange(112, 113); // 200 * (1080/1920) = 112.5 -> ~112-113
        thumb.WebpBytes.Should().NotBeEmpty();

        // Biến thể Medium: max 720px
        var medium = result.Variants.First(v => v.VariantName == "medium");
        medium.Width.Should().Be(720);
        medium.Height.Should().Be(405); // 720 * (1080/1920) = 405
        medium.WebpBytes.Should().NotBeEmpty();

        // Biến thể Large: max 1440px
        var large = result.Variants.First(v => v.VariantName == "large");
        large.Width.Should().Be(1440);
        large.Height.Should().Be(810); // 1440 * (1080/1920) = 810
        large.WebpBytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProcessVariantsAsync_WithSmallerImage_ShouldNotUpscale()
    {
        // Ảnh gốc nhỏ hơn 1440 (ví dụ 600x400)
        var bytes = CreateTestJpeg(600, 400);
        using var stream = new MemoryStream(bytes);

        var result = await _processor.ProcessVariantsAsync(stream, "avatar");

        var large = result.Variants.First(v => v.VariantName == "large");
        // Large không được upscale lên 1440 mà giữ nguyên 600x400
        large.Width.Should().Be(600);
        large.Height.Should().Be(400);

        var thumb = result.Variants.First(v => v.VariantName == "thumb");
        thumb.Width.Should().Be(200);
    }
}
