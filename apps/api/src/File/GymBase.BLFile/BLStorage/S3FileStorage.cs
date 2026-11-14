using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLStorage;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;
    private readonly ILogger<S3FileStorage> _logger;
    private bool _bucketChecked;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);

    public string ProviderName => string.Equals(_options.DefaultProvider, "MINIO", StringComparison.OrdinalIgnoreCase) ? "MINIO" : "S3";

    public S3FileStorage(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
            Headers = { CacheControl = "public, max-age=31536000, immutable" }
        };

        try
        {
            await _s3Client.PutObjectAsync(putRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu object '{Key}' vào {Provider} (Endpoint: {Endpoint}, Bucket: {Bucket}). Đảm bảo MinIO/S3 đang hoạt động.",
                key, ProviderName, _options.ServiceUrl ?? "AWS", _options.BucketName);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        if (_bucketChecked) return;
        await _bucketLock.WaitAsync(ct);
        try
        {
            if (_bucketChecked) return;
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _options.BucketName);
            if (!exists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _options.BucketName }, ct);
                _logger.LogInformation("Đã tự động khởi tạo bucket '{Bucket}' trên MinIO/S3.", _options.BucketName);
            }
            _bucketChecked = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kiểm tra/khởi tạo bucket '{Bucket}'. Tiếp tục lưu với giả định bucket đã sẵn sàng.", _options.BucketName);
            _bucketChecked = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(getRequest, ct);
        return response.ResponseStream;
    }

    public async Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var head = await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = key
            }, ct);

            return new StoredObjectInfo(head.ContentLength, head.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            }, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Bỏ qua nếu đã không tồn tại
        }
    }
}
