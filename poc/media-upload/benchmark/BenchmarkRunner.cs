using System.Diagnostics;
using GymBase.Poc.Media.Processing;
using SkiaSharp;

namespace GymBase.Poc.Media.Benchmark;

public static class BenchmarkRunner
{
    private static readonly (string Name, int Width, int Height, int DetailShapes)[] SampleSpecs =
    [
        ("Ảnh 1: Avatar PT (Chân dung)", 3000, 4000, 200),
        ("Ảnh 2: Cơ sở phòng gym (Landscape)", 4032, 3024, 350),
        ("Ảnh 3: Ảnh tiến độ học viên (Body)", 3456, 4608, 250),
        ("Ảnh 4: Bài tập thiết bị gym (Post)", 4000, 3000, 400),
        ("Ảnh 5: Nhóm tập luyện lớp nhóm (Group)", 4800, 3600, 500)
    ];

    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=========================================================================================================");
        Console.WriteLine("              GYMBASE V1 - BENCHMARK XỬ LÝ ẢNH & SINH BIẾN THỂ (SkiaSharp WebP)");
        Console.WriteLine("=========================================================================================================");
        Console.WriteLine($"Thời điểm chạy: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Runtime: .NET 9.0 (SkiaSharp 2.88.9)");
        Console.WriteLine("Chính sách: Loại bỏ ảnh gốc sau khi confirm -> Chỉ lưu WebP (Thumb 200px, Medium 720px, Large 1440px)\n");

        var processor = new SkiaImageProcessor();

        Console.WriteLine("{0,-38} | {1,-10} | {2,-10} | {3,-10} | {4,-10} | {5,-10} | {6,-8} | {7,-8}",
            "Tên mẫu ảnh (Điện thoại)", "Dung lượng gốc", "Thumb (WebP)", "Medium (WebP)", "Large (WebP)", "Tổng WebP", "Giảm (%)", "Thời gian");
        Console.WriteLine(new string('-', 120));

        long totalOriginalBytes = 0;
        long totalVariantsBytes = 0;
        double totalMilliseconds = 0;

        foreach (var (name, width, height, shapes) in SampleSpecs)
        {
            // 1. Tạo ảnh test chân thực mô phỏng ảnh chụp điện thoại (3MB - 8MB)
            var rawJpegBytes = CreateRealisticPhonePhoto(width, height, shapes);
            var originalSizeKb = rawJpegBytes.Length / 1024.0;
            totalOriginalBytes += rawJpegBytes.Length;

            // 2. Đo thời gian xử lý confirm (magic bytes + decode + resize 3 biến thể + encode WebP + blurhash + hash)
            using var stream = new MemoryStream(rawJpegBytes);

            var sw = Stopwatch.StartNew();
            processor.ValidateMagicBytes(stream, "image/jpeg");
            var result = await processor.ProcessVariantsAsync(stream, "benchmark");
            sw.Stop();

            totalMilliseconds += sw.Elapsed.TotalMilliseconds;

            var thumb = result.Variants.First(v => v.VariantName == "thumb");
            var medium = result.Variants.First(v => v.VariantName == "medium");
            var large = result.Variants.First(v => v.VariantName == "large");

            var thumbKb = thumb.WebpBytes.Length / 1024.0;
            var mediumKb = medium.WebpBytes.Length / 1024.0;
            var largeKb = large.WebpBytes.Length / 1024.0;
            var variantsTotalKb = result.TotalVariantsSizeBytes / 1024.0;
            totalVariantsBytes += result.TotalVariantsSizeBytes;

            var reductionPercent = (1.0 - (double)result.TotalVariantsSizeBytes / rawJpegBytes.Length) * 100.0;

            Console.WriteLine("{0,-38} | {1,8:F1} KB | {2,8:F1} KB | {3,8:F1} KB | {4,8:F1} KB | {5,8:F1} KB | {6,7:F1}% | {7,6:F0} ms",
                $"{name} ({width}x{height})",
                originalSizeKb,
                thumbKb,
                mediumKb,
                largeKb,
                variantsTotalKb,
                reductionPercent,
                sw.Elapsed.TotalMilliseconds);
        }

        Console.WriteLine(new string('-', 120));
        var overallReduction = (1.0 - (double)totalVariantsBytes / totalOriginalBytes) * 100.0;
        Console.WriteLine("TỔNG KẾT: Gốc {0:F2} MB -> Tổng 3 biến thể {1:F2} MB | Tiết kiệm: {2:F1}% dung lượng lưu trữ | TB: {3:F0} ms/ảnh\n",
            totalOriginalBytes / (1024.0 * 1024.0),
            totalVariantsBytes / (1024.0 * 1024.0),
            overallReduction,
            totalMilliseconds / SampleSpecs.Length);
    }

    private static byte[] CreateRealisticPhonePhoto(int width, int height, int shapeCount)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Tạo nền gradient tự nhiên
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, height),
            [SKColors.DarkSlateGray, SKColors.BurlyWood, SKColors.Navy],
            null,
            SKShaderTileMode.Clamp);

        using var bgPaint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        // Tạo các khối hình ảnh và họa tiết chi tiết để mô phỏng độ phức tạp của ảnh thực tế
        var rand = new Random(width ^ height);
        for (var i = 0; i < shapeCount; i++)
        {
            var x = (float)rand.NextDouble() * width;
            var y = (float)rand.NextDouble() * height;
            var radius = 20f + (float)rand.NextDouble() * 120f;
            var color = new SKColor(
                (byte)rand.Next(30, 240),
                (byte)rand.Next(30, 240),
                (byte)rand.Next(30, 240),
                (byte)rand.Next(150, 255));

            using var paint = new SKPaint { Color = color, IsAntialias = true };
            if (i % 2 == 0)
                canvas.DrawCircle(x, y, radius, paint);
            else
                canvas.DrawRect(x, y, radius * 1.5f, radius, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 92);
        return data.ToArray();
    }
}
