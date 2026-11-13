using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLStorage;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;
    private readonly ILogger<S3FileStorage> _logger;

    public string ProviderName => "S3";

    public S3FileStorage(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
            Headers = { CacheControl = "public, max-age=31536000, immutable" }
        };

        await _s3Client.PutObjectAsync(putRequest, ct);
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
