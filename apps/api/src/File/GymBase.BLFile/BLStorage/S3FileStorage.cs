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
    private readonly Lazy<IAmazonS3> _signer;

    public string ProviderName => string.Equals(_options.DefaultProvider, "MINIO", StringComparison.OrdinalIgnoreCase) ? "MINIO" : "S3";

    public S3FileStorage(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
        _signer = new Lazy<IAmazonS3>(CreateSigner);
    }

    public bool SupportsPresign => true;

    /// <summary>
    /// Client dùng để ký URL. Chữ ký gắn với host, nên khi thiết bị gọi MinIO bằng địa chỉ khác (<c>PublicServiceUrl</c>) thì phải ký bằng
    /// chính địa chỉ đó; không có thì dùng client thường.
    /// </summary>
    private IAmazonS3 CreateSigner()
    {
        if (string.IsNullOrWhiteSpace(_options.PublicServiceUrl))
        {
            return _s3Client;
        }

        var config = new AmazonS3Config { ServiceURL = _options.PublicServiceUrl, ForcePathStyle = _options.ForcePathStyle };
        return new AmazonS3Client(new Amazon.Runtime.BasicAWSCredentials(_options.AccessKey, _options.SecretKey), config);
    }

    /// <summary>
    /// <c>GetPreSignedUrlRequest</c> mặc định HTTPS dù endpoint là http (MinIO dev), nên phải đặt theo địa chỉ dùng để ký.
    /// </summary>
    private Protocol GiaoThuc()
    {
        var url = string.IsNullOrWhiteSpace(_options.PublicServiceUrl) ? _options.ServiceUrl : _options.PublicServiceUrl;
        return url != null && url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? Protocol.HTTP : Protocol.HTTPS;
    }

    public PresignedUpload CreatePresignedUpload(string key, string contentType, long maxBytes, TimeSpan expiry)
    {
        var now = DateTime.UtcNow;
        var date = now.ToString("yyyyMMdd");
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'");
        var credential = $"{_options.AccessKey}/{date}/{_options.Region}/s3/aws4_request";

        var policy = new
        {
            expiration = now.Add(expiry).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            conditions = new object[]
            {
                new Dictionary<string, string> { ["bucket"] = _options.BucketName },
                new Dictionary<string, string> { ["key"] = key },
                new Dictionary<string, string> { ["Content-Type"] = contentType },
                new Dictionary<string, string> { ["x-amz-algorithm"] = "AWS4-HMAC-SHA256" },
                new Dictionary<string, string> { ["x-amz-credential"] = credential },
                new Dictionary<string, string> { ["x-amz-date"] = amzDate },
                new object[] { "content-length-range", 1, maxBytes },
            },
        };
        var policyBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(policy)));

        var kSigning = Hmac(Hmac(Hmac(Hmac(System.Text.Encoding.UTF8.GetBytes("AWS4" + _options.SecretKey), date), _options.Region), "s3"), "aws4_request");
        var signature = Convert.ToHexStringLower(Hmac(kSigning, policyBase64));

        var fields = new Dictionary<string, string>
        {
            ["key"] = key,
            ["Content-Type"] = contentType,
            ["x-amz-algorithm"] = "AWS4-HMAC-SHA256",
            ["x-amz-credential"] = credential,
            ["x-amz-date"] = amzDate,
            ["policy"] = policyBase64,
            ["x-amz-signature"] = signature,
        };
        return new PresignedUpload(UrlForm(), fields);
    }

    private static byte[] Hmac(byte[] key, string data) => System.Security.Cryptography.HMACSHA256.HashData(key, System.Text.Encoding.UTF8.GetBytes(data));

    /// <summary>Địa chỉ nhận form POST: path-style <c>{endpoint}/{bucket}</c> khi có endpoint riêng (MinIO), nếu không thì host của bucket trên S3.</summary>
    private string UrlForm()
    {
        var endpoint = string.IsNullOrWhiteSpace(_options.PublicServiceUrl) ? _options.ServiceUrl : _options.PublicServiceUrl;
        return string.IsNullOrWhiteSpace(endpoint)
            ? $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/"
            : $"{endpoint.TrimEnd('/')}/{_options.BucketName}";
    }

    public string CreatePresignedGetUrl(string key, TimeSpan expiry) =>
        _signer.Value.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry),
            Protocol = GiaoThuc(),
        });

    public async Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);
        await _s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = _options.BucketName,
            SourceKey = sourceKey,
            DestinationBucket = _options.BucketName,
            DestinationKey = destinationKey,
        }, ct);
    }

    public async Task<byte[]> ReadHeadAsync(string key, int count, CancellationToken ct = default)
    {
        using var response = await _s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            ByteRange = new ByteRange(0, count - 1),
        }, ct);
        await using var stream = response.ResponseStream;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
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
