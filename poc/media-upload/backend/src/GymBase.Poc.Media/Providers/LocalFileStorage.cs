using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.Configuration;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Providers;

/// <summary>
/// Triển khai lưu trữ file cục bộ trên ổ đĩa máy chủ hoặc Docker volume (/app/uploads).
/// Có kiểm tra chống Path Traversal nghiêm ngặt theo tài liệu thi công GymBase V1.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    public string ProviderName => "LOCAL";

    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        var localRoot = options.Value.LocalRoot;
        if (string.IsNullOrWhiteSpace(localRoot))
        {
            localRoot = "uploads";
        }

        _root = Path.GetFullPath(localRoot);
        Directory.CreateDirectory(_root);
    }

    public string ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key) ||
            key.Contains('\\') ||
            key.StartsWith('/') ||
            key.Split('/').Any(s => s is "" or "." or ".."))
        {
            throw new ArgumentException("Invalid storage key: path traversal or invalid characters detected.");
        }

        var full = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSep = _root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootWithSep, StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid storage key: path resolves outside of storage root.");
        }

        return full;
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var path = ResolvePath(key);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Ghi bằng FileStream async, tránh ghi đè ngẫu nhiên
        await using var output = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        content.Position = 0;
        await content.CopyToAsync(output, ct);
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default)
    {
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Local file does not exist: {key}");
        }

        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult(stream);
    }

    public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default)
    {
        var path = ResolvePath(key);
        var file = new FileInfo(path);

        if (!file.Exists)
        {
            return Task.FromResult<StoredObjectInfo?>(null);
        }

        return Task.FromResult<StoredObjectInfo?>(new StoredObjectInfo(file.Length, "application/octet-stream"));
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var path = ResolvePath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        return Task.CompletedTask;
    }
}
