using GymBase.BLFile.BLProcessing;
using SkiaSharp;
using Xunit;

namespace GymBase.UnitTests.File;

/// <summary>Ảnh mẫu dựng bằng Skia: nửa trái đỏ, nửa phải xanh, để kiểm hướng xoay theo EXIF.</summary>
internal static class MauAnh
{
    public static byte[] Tao(int rong, int cao, SKEncodedImageFormat dinhDang = SKEncodedImageFormat.Jpeg)
    {
        using var bitmap = new SKBitmap(rong, cao);
        using (var canvas = new SKCanvas(bitmap))
        {
            using var doPaint = new SKPaint { Color = SKColors.Red };
            using var xanhPaint = new SKPaint { Color = SKColors.Blue };
            canvas.DrawRect(0, 0, rong / 2f, cao, doPaint);
            canvas.DrawRect(rong / 2f, 0, rong - rong / 2f, cao, xanhPaint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        return image.Encode(dinhDang, 95).ToArray();
    }

    /// <summary>Chèn đoạn EXIF (APP1) có Orientation và một tag GPS giả vào sau SOI của JPEG.</summary>
    public static byte[] VoiExif(byte[] jpeg, int orientation)
    {
        byte[] tiff =
        [
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, (byte)orientation, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        ];
        byte[] payload = [.. "Exif\0\0"u8.ToArray(), .. tiff, .. "GPS-LAT-21.03N"u8.ToArray()];
        var length = payload.Length + 2;
        byte[] segment = [0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. payload];
        return [.. jpeg[..2], .. segment, .. jpeg[2..]];
    }

    public static bool CoChuoi(byte[] data, string chuoi) =>
        data.AsSpan().IndexOf(System.Text.Encoding.ASCII.GetBytes(chuoi)) >= 0;
}

public sealed class SkiaImageProcessorTests
{
    private readonly SkiaImageProcessor _processor = new();

    private Task<ProcessedImage> XuLy(byte[] data, string mime) => _processor.XuLyAsync(new MemoryStream(data), mime);

    private static SKColor Diem(byte[] data, int x, int y)
    {
        using var bitmap = SKBitmap.Decode(data);
        return bitmap.GetPixel(x, y);
    }

    [Fact]
    public async Task Jpeg_ExifAndGpsAreRemoved_AndBlurhashIsComputed()
    {
        var goc = MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 1);
        Assert.True(MauAnh.CoChuoi(goc, "Exif"));
        Assert.True(MauAnh.CoChuoi(goc, "GPS-LAT"));

        var kq = await XuLy(goc, "image/jpeg");

        Assert.False(MauAnh.CoChuoi(kq.Data, "Exif"));
        Assert.False(MauAnh.CoChuoi(kq.Data, "GPS-LAT"));
        Assert.Equal((20, 10), (kq.Width, kq.Height));
        Assert.True(kq.Blurhash.Length >= 6);
        Assert.Equal(0xFF, kq.Data[0]);
        Assert.Equal(0xD8, kq.Data[1]);
    }

    [Fact]
    public async Task Jpeg_Orientation6_IsRotatedClockwiseSoLeftBecomesTop()
    {
        var goc = MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 6);

        var kq = await XuLy(goc, "image/jpeg");

        Assert.Equal((10, 20), (kq.Width, kq.Height));
        var tren = Diem(kq.Data, 5, 2);
        var duoi = Diem(kq.Data, 5, 17);
        Assert.True(tren.Red > 200 && tren.Blue < 60, $"Phía trên phải đỏ, thực tế {tren}");
        Assert.True(duoi.Blue > 200 && duoi.Red < 60, $"Phía dưới phải xanh, thực tế {duoi}");
    }

    [Fact]
    public async Task Jpeg_Orientation3_IsRotated180()
    {
        var kq = await XuLy(MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 3), "image/jpeg");

        Assert.Equal((20, 10), (kq.Width, kq.Height));
        Assert.True(Diem(kq.Data, 3, 5).Blue > 200);
        Assert.True(Diem(kq.Data, 16, 5).Red > 200);
    }

    [Fact]
    public async Task Jpeg_Orientation8_IsRotatedCounterClockwiseSoLeftBecomesBottom()
    {
        var kq = await XuLy(MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 8), "image/jpeg");

        Assert.Equal((10, 20), (kq.Width, kq.Height));
        Assert.True(Diem(kq.Data, 5, 17).Red > 200);
        Assert.True(Diem(kq.Data, 5, 2).Blue > 200);
    }

    [Fact]
    public async Task Jpeg_Orientation2_IsMirroredHorizontally()
    {
        var kq = await XuLy(MauAnh.VoiExif(MauAnh.Tao(20, 10), orientation: 2), "image/jpeg");

        Assert.Equal((20, 10), (kq.Width, kq.Height));
        Assert.True(Diem(kq.Data, 3, 5).Blue > 200);
    }

    [Theory]
    [InlineData("image/png", SKEncodedImageFormat.Png)]
    [InlineData("image/webp", SKEncodedImageFormat.Webp)]
    public async Task PngAndWebp_KeepTheirFormat(string mime, SKEncodedImageFormat dinhDang)
    {
        var kq = await XuLy(MauAnh.Tao(16, 16, dinhDang), mime);

        using var codec = SKCodec.Create(new MemoryStream(kq.Data));
        Assert.Equal(dinhDang, codec.EncodedFormat);
    }

    [Fact]
    public async Task OversizedImage_IsRejectedBeforeDecoding()
    {
        var qualon = MauAnh.Tao(8193, 2, SKEncodedImageFormat.Png);

        await Assert.ThrowsAsync<ImageTooLargeException>(() => XuLy(qualon, "image/png"));
    }

    [Fact]
    public async Task CorruptData_And_UnsupportedType_ThrowInvalidData()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => XuLy([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12], "image/jpeg"));
        await Assert.ThrowsAsync<InvalidDataException>(() => XuLy(MauAnh.Tao(8, 8), "image/heic"));
    }
}
