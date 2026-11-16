using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using GymBase.BLFile.BLFiles;
using GymBase.BLFile.BLNotifications;
using GymBase.BLFile.BLProcessing;
using GymBase.BLFile.BLStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile;

public static class FileBusinessExtensions
{
    public static IServiceCollection AddFileBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Cấu hình StorageOptions & NotificationOptions
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));

        // 2. Image Processing
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

        // 3. Storage Providers
        services.AddSingleton<LocalFileStorage>();

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

            AWSCredentials credentials = !string.IsNullOrWhiteSpace(options.AccessKey) && !string.IsNullOrWhiteSpace(options.SecretKey)
                ? new BasicAWSCredentials(options.AccessKey, options.SecretKey)
                : new AnonymousAWSCredentials();

            return new AmazonS3Client(credentials, s3Config);
        });

        services.AddSingleton<S3FileStorage>();
        services.AddSingleton<IStorageResolver, StorageResolver>();

        // 4. File Services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileMigrationService, FileMigrationService>();

        // 5. Notification Services
        services.AddSingleton<IPushNotificationService, FcmV1PushNotificationService>();

        return services;
    }
}
