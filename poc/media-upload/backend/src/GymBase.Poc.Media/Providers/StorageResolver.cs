using GymBase.Poc.Media.Abstractions;
using GymBase.Poc.Media.Configuration;
using Microsoft.Extensions.Options;

namespace GymBase.Poc.Media.Providers;

/// <summary>
/// Định tuyến Storage Provider dựa trên cột nha_cung_cap của từng dòng tep_tin.
/// Cho phép hệ thống cùng lúc vừa đọc các file cũ ở LOCAL, vừa ghi/đọc các file mới ở S3.
/// </summary>
public sealed class StorageResolver : IStorageResolver
{
    private readonly IFileStorage _localStorage;
    private readonly IFileStorage _s3Storage;
    private readonly StorageOptions _options;

    public StorageResolver(
        LocalFileStorage localStorage,
        S3FileStorage s3Storage,
        IOptions<StorageOptions> options)
    {
        _localStorage = localStorage;
        _s3Storage = s3Storage;
        _options = options.Value;
    }

    public IFileStorage Resolve(string providerName)
    {
        if (string.Equals(providerName, "LOCAL", StringComparison.OrdinalIgnoreCase))
        {
            return _localStorage;
        }

        if (string.Equals(providerName, "S3", StringComparison.OrdinalIgnoreCase))
        {
            return _s3Storage;
        }

        throw new NotSupportedException($"Nhà cung cấp lưu trữ '{providerName}' không được hỗ trợ.");
    }

    public IFileStorage GetDefault()
    {
        return Resolve(_options.DefaultProvider);
    }
}
