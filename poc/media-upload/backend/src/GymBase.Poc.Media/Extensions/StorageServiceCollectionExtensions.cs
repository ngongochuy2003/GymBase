using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.BL;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Model;
using GymBase.Poc.Media.Processing;
using GymBase.Poc.Media.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Extensions;

public static class StorageServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký toàn bộ hạ tầng lưu trữ chuẩn V1: Local Storage + S3 Ready.
    /// Dễ dàng tích hợp vào GymBase.Api/Program.cs chỉ với 1 dòng lệnh.
    /// </summary>
    public static IServiceCollection AddGymBaseFileStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<StorageOptions>? configureOptions = null)
    {
        // 1. Cấu hình StorageOptions
        var storageSection = configuration.GetSection(StorageOptions.SectionName);
        services.Configure<StorageOptions>(storageSection);

        if (configureOptions != null)
        {
            services.PostConfigure(configureOptions);
        }

        // 2. Đăng ký Image Processor (SkiaSharp WebP + Blurhash)
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

        // 3. Đăng ký Repository metadata bảng tep_tin (In-Memory cho POC, map sang EF Core ở DL)
        services.AddSingleton<ITepTinRepository, InMemoryTepTinRepository>();

        // 4. Đăng ký Local Storage Provider (ổ đĩa / Docker volume /app/uploads)
        services.AddSingleton<LocalFileStorage>();

        // 5. Đăng ký S3 Provider (Amazon S3 / MinIO)
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var s3Config = new AmazonS3Config();

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                s3Config.ServiceURL = options.ServiceUrl;
                s3Config.ForcePathStyle = options.ForcePathStyle;
            }
            else
            {
                s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            var accessKey = configuration["AWS:AccessKey"]
                            ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
                            ?? "minioadmin";
            var secretKey = configuration["AWS:SecretKey"]
                            ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                            ?? "minioadmin";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            return new AmazonS3Client(credentials, s3Config);
        });

        services.AddSingleton<S3FileStorage>();

        // 6. Đăng ký Storage Resolver (chọn LOCAL hay S3 theo nha_cung_cap)
        services.AddSingleton<IStorageResolver, StorageResolver>();

        // 7. Đăng ký Nghiệp vụ FileService & FileMigrationService
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileMigrationService, FileMigrationService>();

        return services;
    }
}
