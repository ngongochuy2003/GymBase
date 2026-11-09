using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Processing;
using GymBase.Poc.Media.Services;
using GymBase.Poc.Media.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Extensions;

public static class MediaServiceCollectionExtensions
{
    /// <summary>
    /// Điểm đăng ký DI duy nhất cho module Media Upload.
    /// Dễ dàng tích hợp vào GymBase.Api/Program.cs chỉ với 1 dòng lệnh.
    /// </summary>
    public static IServiceCollection AddMediaUpload(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MediaOptions>? configureOptions = null)
    {
        // 1. Cấu hình MediaOptions
        var mediaSection = configuration.GetSection(MediaOptions.SectionName);
        services.Configure<MediaOptions>(mediaSection);

        if (configureOptions != null)
        {
            services.PostConfigure(configureOptions);
        }

        // 2. Đăng ký Image Processor (SkiaSharp)
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

        // 3. Đăng ký Storage Adapter (S3 hoặc InMemory tùy cấu hình)
        services.AddSingleton<IMediaStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MediaOptions>>().Value;

            if (options.UseInMemoryStorage)
            {
                return new InMemoryMediaStorage();
            }

            // Cấu hình AWS S3 Client
            var s3Config = new AmazonS3Config();

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                // Dùng MinIO hoặc LocalStack
                s3Config.ServiceURL = options.ServiceUrl;
                s3Config.ForcePathStyle = options.ForcePathStyle;
            }
            else
            {
                // Dùng AWS S3 chuẩn
                s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            // Đọc credentials từ ENV / appsettings / AWS profile
            var accessKey = configuration["AWS:AccessKey"] ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "minioadmin";
            var secretKey = configuration["AWS:SecretKey"] ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "minioadmin";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var amazonS3 = new AmazonS3Client(credentials, s3Config);

            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<S3MediaStorage>>();
            return new S3MediaStorage(amazonS3, sp.GetRequiredService<IOptions<MediaOptions>>(), logger);
        });

        // 4. Đăng ký MediaService
        services.AddScoped<IMediaService, MediaService>();

        return services;
    }
}
