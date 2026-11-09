using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using GymBase.Poc.Media.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Storage;

/// <summary>
/// Triển khai IMediaStorage dựa trên Amazon S3 (hoặc MinIO tương thích S3).
/// </summary>
public sealed class S3MediaStorage : IMediaStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly MediaOptions _options;
    private readonly ILogger<S3MediaStorage> _logger;

    public S3MediaStorage(
        IAmazonS3 s3Client,
        IOptions<MediaOptions> options,
        ILogger<S3MediaStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public Task<string> GetPresignedPutUrlAsync(
        string key,
        string contentType,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        };

        // Ký URL với Content-Type để ngăn chặn mobile thay đổi mime-type trái phép khi PUT
        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public Task<string> GetPresignedGetUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task<StorageObjectMetadata?> HeadObjectAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(_options.BucketName, key, ct);
            return new StorageObjectMetadata(
                Key: key,
                SizeBytes: response.ContentLength,
                ContentType: response.Headers.ContentType ?? "application/octet-stream",
                LastModified: response.LastModified,
                ETag: response.ETag,
                CacheControl: response.Headers.CacheControl
            );
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi HeadObject S3 với key: {Key}", key);
            throw;
        }
    }

    public async Task<Stream> GetObjectStreamAsync(string key, CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_options.BucketName, key, ct);
        // Copy vào MemoryStream để không giữ mở kết nối HTTP network S3 lâu
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task PutObjectAsync(
        string key,
        Stream data,
        string contentType,
        string cacheControl,
        CancellationToken ct = default)
    {
        data.Position = 0;
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = data,
            ContentType = contentType,
            AutoCloseStream = false
        };

        if (!string.IsNullOrWhiteSpace(cacheControl))
        {
            request.Headers.CacheControl = cacheControl;
        }

        await _s3Client.PutObjectAsync(request, ct);
    }

    public async Task DeleteObjectAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_options.BucketName, key, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Bỏ qua nếu object đã không còn tồn tại
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var meta = await HeadObjectAsync(key, ct);
        return meta is not null;
    }
}
