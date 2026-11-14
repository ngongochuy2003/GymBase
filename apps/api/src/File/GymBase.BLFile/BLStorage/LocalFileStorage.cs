using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLStorage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly string _absoluteRoot;

    public string ProviderName => "LOCAL";

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        _absoluteRoot = Path.GetFullPath(string.IsNullOrWhiteSpace(_options.LocalRoot) ? "uploads" : _options.LocalRoot);

        if (!Directory.Exists(_absoluteRoot))
        {
            Directory.CreateDirectory(_absoluteRoot);
        }
    }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var physicalPath = ResolveAndValidatePath(key);
        var dir = Path.GetDirectoryName(physicalPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(fileStream, ct);
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default)
    {
        var physicalPath = ResolveAndValidatePath(key);
        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException($"Tệp tin cục bộ không tồn tại: {key}");
        }

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream>(stream);
    }

    public Task<StoredObjectInfo?> GetInfoAsync(string key, CancellationToken ct = default)
    {
        var physicalPath = ResolveAndValidatePath(key);
        if (!File.Exists(physicalPath))
        {
            return Task.FromResult<StoredObjectInfo?>(null);
        }

        var fi = new FileInfo(physicalPath);
        var ext = fi.Extension.ToLowerInvariant();
        var contentType = ext switch
        {
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return Task.FromResult<StoredObjectInfo?>(new StoredObjectInfo(fi.Length, contentType));
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var physicalPath = ResolveAndValidatePath(key);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    public string ResolveAndValidatePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Storage key không được để trống.", nameof(key));
        }

        if (key.Contains('\\') || key.Contains(':') || key.StartsWith('/'))
        {
            throw new UnauthorizedAccessException($"Storage key chứa ký tự cấm hoặc đường dẫn tuyệt đối: {key}");
        }

        var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(s => s == ".." || s == "."))
        {
            throw new UnauthorizedAccessException($"Phát hiện tấn công Path Traversal trong storage key: {key}");
        }

        var combined = Path.Combine(_absoluteRoot, Path.Combine(segments));
        var fullPath = Path.GetFullPath(combined);

        var rootWithSep = _absoluteRoot.EndsWith(Path.DirectorySeparatorChar)
            ? _absoluteRoot
            : _absoluteRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase) && !fullPath.Equals(_absoluteRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Đường dẫn vượt khỏi thư mục lưu trữ cho phép: {key}");
        }

        return fullPath;
    }
}
