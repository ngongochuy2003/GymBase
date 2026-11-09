using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Providers;

/// <summary>
/// Triển khai IFileStorage cho Amazon S3 / MinIO.
/// Sẵn sàng chuyển đổi khi dự án nâng cấp từ Local Storage lên Cloud Object Storage.
/// </summary>
public sealed class S3FileStorage : IFileStorage
{
    public string ProviderName => "S3";

    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;
    private readonly ILogger<S3FileStorage> _logger;

    public S3FileStorage(
        IAmazonS3 s3Client,
        IOptions<StorageOptions> options,
        ILogger<S3FileStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        content.Position = 0;
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, ct);
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(_options.BucketName, key, ct);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"S3 object not found: {key}", ex);
        }
    }

    public async Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(_options.BucketName, key, ct);
            return new StoredObjectInfo(
                Length: response.ContentLength,
                ContentType: response.Headers.ContentType ?? "application/octet-stream");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_options.BucketName, key, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Bỏ qua nếu object đã không còn trên S3
        }
    }
}
