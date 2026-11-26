using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using Amazon.Runtime;
using Amazon.S3;
using GymBase.BLFile.BLStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.File;

/// <summary>
/// Chạy với MinIO thật ở <c>localhost:9000</c> (<c>minioadmin</c>), kiểm presigned URL ràng buộc đúng Content-Type và Content-Length
/// mà mock không kiểm được. MinIO không chạy thì các test thoát ngay (không tính là lỗi); đặt GYMBASE_MINIO_TEST=1 để bắt buộc phải có MinIO.
/// </summary>
public sealed class S3FileStorageMinioTests : IAsyncLifetime
{
    private const string Endpoint = "http://localhost:9000";
    private readonly string _bucket = "gymbase-test-" + Guid.NewGuid().ToString("N")[..12];
    private readonly HttpClient _http = new();
    private AmazonS3Client? _client;
    private S3FileStorage? _storage;
    private bool _available;

    public async Task InitializeAsync()
    {
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync("localhost", 9000).WaitAsync(TimeSpan.FromMilliseconds(800));
            _available = true;
        }
        catch (Exception) when (Environment.GetEnvironmentVariable("GYMBASE_MINIO_TEST") != "1")
        {
            return;
        }

        _client = new AmazonS3Client(
            new BasicAWSCredentials("minioadmin", "minioadmin"),
            new AmazonS3Config { ServiceURL = Endpoint, ForcePathStyle = true });
        await _client.PutBucketAsync(_bucket);
        _storage = new S3FileStorage(
            _client,
            Options.Create(new StorageOptions
            {
                DefaultProvider = "MINIO", BucketName = _bucket, ServiceUrl = Endpoint, AccessKey = "minioadmin", SecretKey = "minioadmin",
            }),
            NullLogger<S3FileStorage>.Instance);
    }

    public async Task DisposeAsync()
    {
        if (_client != null)
        {
            var objects = await _client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request { BucketName = _bucket });
            foreach (var o in objects.S3Objects)
            {
                await _client.DeleteObjectAsync(_bucket, o.Key);
            }

            await _client.DeleteBucketAsync(_bucket);
            _client.Dispose();
        }

        _http.Dispose();
    }

    private static readonly byte[] Data = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];

    /// <summary>Gửi form như app sẽ gửi: các field của policy trước, file sau cùng.</summary>
    private async Task<HttpStatusCode> PostAsync(PresignedUpload form, byte[] body, Action<Dictionary<string, string>>? doiField = null)
    {
        var fields = new Dictionary<string, string>(form.Fields);
        doiField?.Invoke(fields);
        using var content = new MultipartFormDataContent();
        foreach (var (name, value) in fields)
        {
            var part = new StringContent(value);
            part.Headers.ContentType = null;
            content.Add(part, name);
        }

        var file = new ByteArrayContent(body);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(file, "file", "f.jpg");
        using var response = await _http.PostAsync(form.Url, content);
        return response.StatusCode;
    }

    private static bool Ok(HttpStatusCode status) => (int)status is >= 200 and < 300;

    [Fact]
    public async Task PresignedUpload_WithDeclaredTypeAndLength_Succeeds_AndCopyHeadAndGetWork()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/a", "image/jpeg", 1024, TimeSpan.FromMinutes(5));

        Assert.True(Ok(await PostAsync(form, Data)));

        var info = await _storage.GetInfoAsync("tmp/a");
        Assert.Equal(Data.Length, info!.Length);
        Assert.Equal("image/jpeg", info.ContentType);
        Assert.Equal(Data[..16], await _storage.ReadHeadAsync("tmp/a", 16));

        await _storage.CopyAsync("tmp/a", "avatar/2026/11/a");
        await _storage.DeleteAsync("tmp/a");
        Assert.Null(await _storage.GetInfoAsync("tmp/a"));
        Assert.Equal(Data.Length, (await _storage.GetInfoAsync("avatar/2026/11/a"))!.Length);

        var getUrl = _storage.CreatePresignedGetUrl("avatar/2026/11/a", TimeSpan.FromMinutes(5));
        Assert.Equal(Data, await _http.GetByteArrayAsync(getUrl));
    }

    [Fact]
    public async Task PresignedUpload_WithOtherContentType_IsRejected()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/b", "image/jpeg", 1024, TimeSpan.FromMinutes(5));

        var status = await PostAsync(form, Data, f => f["Content-Type"] = "application/pdf");

        Assert.False(Ok(status));
        Assert.Null(await _storage.GetInfoAsync("tmp/b"));
    }

    [Fact]
    public async Task PresignedUpload_WithLargerBodyThanMaximum_IsRejected()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/c", "image/jpeg", Data.Length, TimeSpan.FromMinutes(5));

        var status = await PostAsync(form, [.. Data, .. Data]);

        Assert.False(Ok(status));
        Assert.Null(await _storage.GetInfoAsync("tmp/c"));
    }

    [Fact]
    public async Task PresignedUpload_WithSmallerBodyThanMaximum_IsAccepted_AndEmptyBodyIsRejected()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/d", "image/jpeg", 1024, TimeSpan.FromMinutes(5));
        Assert.True(Ok(await PostAsync(form, Data[..5])));
        Assert.Equal(5, (await _storage.GetInfoAsync("tmp/d"))!.Length);

        var empty = _storage.CreatePresignedUpload("tmp/d2", "image/jpeg", 1024, TimeSpan.FromMinutes(5));
        Assert.False(Ok(await PostAsync(empty, [])));
    }

    [Fact]
    public async Task PresignedUpload_WithOtherKey_IsRejected()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/e", "image/jpeg", 1024, TimeSpan.FromMinutes(5));

        var status = await PostAsync(form, Data, f => f["key"] = "avatar/ghi-de");

        Assert.False(Ok(status));
        Assert.Null(await _storage.GetInfoAsync("avatar/ghi-de"));
    }

    [Fact]
    public async Task PresignedUpload_AfterExpiry_IsRejected()
    {
        if (!_available) return;
        var form = _storage!.CreatePresignedUpload("tmp/f", "image/jpeg", 1024, TimeSpan.FromSeconds(1));
        await Task.Delay(2500);

        Assert.False(Ok(await PostAsync(form, Data)));
    }

    [Fact]
    public async Task PresignedGet_AfterExpiry_IsRejected()
    {
        if (!_available) return;
        await using (var ms = new MemoryStream(Data))
        {
            await _storage!.SaveAsync("x/obj", ms, "image/jpeg");
        }

        var url = _storage!.CreatePresignedGetUrl("x/obj", TimeSpan.FromSeconds(1));
        await Task.Delay(2500);

        using var response = await _http.GetAsync(url);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
